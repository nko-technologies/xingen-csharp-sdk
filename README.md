# Xingen.Sdk

.NET client SDK for the [Xingen](https://xingen.de) e-invoice validation API — submit UBL, CII,
ZUGFeRD, and SAP IDoc/OData invoices for validation against EN16931, XRechnung, and Peppol.

Targets .NET 8+. Built on `System.Net.Http.HttpClient` and `System.Text.Json` — no third-party
dependencies.

> Status: v0.2, covering invoice submission/validation, AI PDF extraction, invoice correction, and
> API key management. Contacts and dashboard/user endpoints are not exposed (they're
> Firebase-auth-only on the backend).

## Install

```bash
dotnet add package Xingen.Sdk
```

## Authentication

Every request needs an API key (`xgn_live_...` for production, `xgn_test_...` for sandbox — sandbox
requests never count toward quota). Create one from the Xingen dashboard or via `client.ApiKeys`.

```csharp
using var client = new XingenClient(Environment.GetEnvironmentVariable("XINGEN_API_KEY")!);
```

`XingenClient` holds one connection-pooled `HttpClient` — construct it once and reuse it, don't
rebuild it per request; dispose it when your application shuts down. The `baseUrl` parameter
overrides the default `https://app.xingen.de/api`, useful for self-hosted or local
(`./gradlew bootRun`, port 10001) testing:

```csharp
using var client = new XingenClient(apiKey, baseUrl: new Uri("http://localhost:10001/api"));
```

## Validate a file

Every validate/submit endpoint is asynchronous — the backend queues the invoice and returns
immediately. Use a `*AndWaitAsync` helper to submit and poll for the result in one call:

```csharp
var result = await client.Invoices.ValidateFileAndWaitAsync(
    "invoice.xml", ValidationProfile.XRECHNUNG);

if (result.Status == InvoiceStatus.Validated && result.ValidationResult!.Valid)
{
    Console.WriteLine("Valid!");
}
else
{
    foreach (var error in result.ValidationResult!.Errors)
    {
        Console.WriteLine($"{error.Severity}: {error.Message} ({error.Field})");
    }
}
```

`PollOptions` controls the backoff (`InitialInterval`, `MaxInterval`, `BackoffMultiplier`) and the
overall `Timeout`. A **failed validation is not an exception** — it's a completed API call that
found the invoice invalid, so `*AndWaitAsync` returns normally with `ValidationResult.Valid ==
false`. Only a transport failure, timeout, or cancellation throws. Every method also accepts an
optional trailing `CancellationToken`.

```csharp
var options = PollOptions.Default with
{
    InitialInterval = TimeSpan.FromMilliseconds(300),
    MaxInterval = TimeSpan.FromSeconds(3),
    Timeout = TimeSpan.FromSeconds(30),
};

var result = await client.Invoices.ValidateFileAndWaitAsync("invoice.xml", ValidationProfile.XRECHNUNG, options);
```

If you'd rather manage polling yourself, use the low-level pair:

```csharp
var submitted = await client.Invoices.ValidateFileAsync("invoice.xml", ValidationProfile.EN16931);
// ... later ...
var record = await client.Invoices.GetAsync(submitted.Id!);
```

`ValidateIdocAsync` / `ValidateIdocAndWaitAsync` work the same way for SAP IDoc XML files. Both also
have `(string filename, byte[] content, ...)` overloads if you already hold the file bytes in
memory instead of a file path.

## Submit a structured invoice (JSON)

```csharp
var submission = new InvoiceSubmission
{
    InvoiceNumber = "INV-2024-0042",
    IssueDate = new DateOnly(2024, 3, 15),
    Currency = "EUR",
    BuyerReference = "991-12345-06",
    ValidationProfile = ValidationProfile.XRECHNUNG,
    Supplier = new InvoiceSubmission.PartyInput
    {
        Name = "Acme GmbH", VatId = "DE123456789",
        Address = new InvoiceSubmission.AddressInput { City = "Berlin", CountryCode = "DE" },
    },
    Buyer = new InvoiceSubmission.PartyInput
    {
        Name = "Buyer Co", LeitwegId = "991-12345-06",
        Address = new InvoiceSubmission.AddressInput { CountryCode = "DE" },
    },
    Lines =
    [
        new InvoiceSubmission.LineInput
        {
            Description = "Software License Q1", Quantity = 5m, Unit = "C62", Price = 199.00m, TaxRate = 19m,
        },
    ],
    PaymentMeans =
    [
        new InvoiceSubmission.PaymentMeansInput
        {
            TypeCode = "58", CreditTransferAccountId = "DE89370400440532013000",
        },
    ],
};

var result = await client.Invoices.SubmitAndWaitAsync(submission);
```

`InvoiceSubmission` has full parity with the backend's domain model — every invoice type it can
validate, it can also submit, including non-standard VAT categories (exempt/reverse-charge/export
via `LineInput.TaxCategoryCode` + `ExemptionReason`/`ExemptionReasonCode`), `Payee`/
`TaxRepresentative`, `Delivery`, `InvoicePeriod`, `PrecedingInvoiceReferences` (for credit notes),
and the full BT-11..BT-19 document reference properties. See the nested records on
`InvoiceSubmission` for the complete set.

SAP S/4HANA OData supplier-invoice payloads are supported as a thin passthrough — pass raw JSON or
a `Dictionary<string, object>` rather than a fully typed model:

```csharp
await client.Invoices.SubmitODataAsync(rawODataJson, ValidationProfile.EN16931);
```

## Extract an invoice from a PDF (AI)

Upload a plain invoice PDF — including scanned/image-based PDFs — and let the backend extract
structured fields. Works exactly like the other submit endpoints: async, so use
`ExtractInvoiceAndWaitAsync` or the low-level `ExtractInvoiceAsync`/`GetAsync` pair.

```csharp
var result = await client.Invoices.ExtractInvoiceAndWaitAsync(
    "scanned-invoice.pdf",
    ValidationProfile.EN16931,
    ExtractionModelTier.FAST); // or ACCURATE — higher accuracy, Pro subscription required
```

If the extraction missed a field or validation flagged something, correct it with a JSON
merge-patch (RFC 7386) and re-validate synchronously — only invoices that finished processing
(`Validated` or `FailedValidation`) can be corrected. Array fields (`lines`, `paymentMeans`,
`allowanceCharges`, `taxBreakdowns`) are replaced wholesale when present in the patch:

```csharp
var corrected = await client.Invoices.PatchInvoiceAsync(result.Id!, new Dictionary<string, object?>
{
    ["currency"] = "EUR",
    ["buyerReference"] = "991-12345-06",
});
```

To find out which fields the backend fills in automatically per profile (so you know what *not*
to prompt the user for):

```csharp
Dictionary<string, List<AutoFilledField>> autoFilled = await client.Invoices.GetAutoFilledFieldsAsync();
```

## List and retrieve invoices

```csharp
var page = await client.Invoices.ListAsync(0, 20, "createdAt,desc");

// or, to walk every invoice without managing page indices yourself:
await foreach (var record in client.Invoices.ListAllAsync(50))
{
    Console.WriteLine($"{record.Id} -> {record.Status}");
}

var one = await client.Invoices.GetAsync("inv_01HXYZ");
```

## Download results

```csharp
byte[] pdf = await client.Invoices.DownloadPdfAsync(id);        // ZUGFeRD PDF with embedded XML
byte[] idocXml = await client.Invoices.DownloadIdocXmlAsync(id); // SAP IDoc XML
```

## API keys

```csharp
var created = await client.ApiKeys.CreateAsync(new CreateApiKeyRequest { Name = "Production CI" });
Console.WriteLine($"Store this now, it's shown only once: {created.RawKey}");

var keys = await client.ApiKeys.ListAsync();
await client.ApiKeys.RevokeAsync(created.Id);
```

## Error handling

All SDK exceptions extend `XingenException`. HTTP errors map to typed subtypes of `ApiException`:

| Exception | Status | Notes |
|---|---|---|
| `AuthenticationException` | 401 | Missing or invalid API key |
| `PermissionException` | 403 | Resource exists but isn't owned by the caller |
| `NotFoundException` | 404 | |
| `ValidationRequestException` | 400 | `FieldErrors` has details for request-body validation failures |
| `QuotaExceededException` | 429 | Monthly request quota exhausted |
| `ApiException` | other 4xx/5xx | Fallback; `StatusCode` / `RawBody` always available |

```csharp
try
{
    await client.Invoices.SubmitAsync(submission);
}
catch (ValidationRequestException e)
{
    foreach (var (field, message) in e.FieldErrors)
    {
        Console.WriteLine($"{field}: {message}");
    }
}
catch (QuotaExceededException)
{
    Console.WriteLine("Quota exceeded — upgrade or wait for the next billing period");
}
catch (XingenException e)
{
    Console.WriteLine($"Request failed: {e.Message}");
}
```

A `*AndWaitAsync` timeout throws `XingenTimeoutException`, whose `PartialResult` carries the last
known `InvoiceRecord` before the deadline elapsed. Cancelling the `CancellationToken` you pass in
throws the standard `OperationCanceledException` rather than an SDK-specific type.

## Design notes

- **No automatic retries.** Retrying a `SubmitAsync` call after a client-side timeout is unsafe
  without idempotency keys, which the API doesn't support yet — a retried submit could create a
  duplicate invoice. Handle retries at the call site if you need them.
- **Async-only, `Task`-based.** This mirrors standard .NET HTTP client conventions; there's no
  synchronous surface, since blocking on async I/O is an anti-pattern in .NET (it risks deadlocks in
  UI/ASP.NET synchronization contexts).
- **`decimal`, not `string`, for monetary/quantity fields.** Unlike some other language runtimes,
  `System.Text.Json` parses JSON numbers directly into `decimal` without a lossy `double`
  intermediate step when the target property is `decimal`-typed, so full precision is preserved
  without extra plumbing — see `JsonCodecTests` for a test that pins this down empirically.

## Contributing

```bash
dotnet test
```

Tests run against a real (loopback) `System.Net.HttpListener`, not a mocking framework — see
`LoopbackServer` in the test project.

## Reference implementation

This SDK is a hand-written, idiom-adapted port of
[`xingen-java-sdk`](https://github.com/nko-technologies/xingen-java-sdk), which is the canonical
reference for this API's behavior (query-param vs. form-field quirks, per-status error body shapes,
etc.) across every language SDK.
