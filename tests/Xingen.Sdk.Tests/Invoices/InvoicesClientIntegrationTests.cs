using Xingen.Sdk;
using Xingen.Sdk.Invoices;
using Xingen.Sdk.Models;
using Xunit;

namespace Xingen.Sdk.Tests.Invoices;

public class InvoicesClientIntegrationTests : IAsyncLifetime
{
    private const string ValidatePath = "/v1/invoices/validate";
    private const string Fixture = "{\"id\":\"inv_01HXYZ\",\"status\":\"validated\","
        + "\"createdAt\":\"2026-07-08T09:30:00Z\",\"validationProfile\":\"XRECHNUNG\",\"invoiceFormat\":\"UBL\","
        + "\"uploadedBy\":\"user_abc123\",\"sandbox\":false,\"apiKeyId\":\"3fa85f64-5717-4562-b3fc-2c963f66afa6\","
        + "\"canonicalJson\":{\"invoiceNumber\":\"INV-2024-0042\",\"currency\":\"EUR\",\"lines\":[],\"notes\":[]},"
        + "\"validationResult\":{\"valid\":true,\"errors\":[],\"kositResult\":null}}";

    private LoopbackServer _server = null!;
    private XingenClient _client = null!;

    public Task InitializeAsync()
    {
        _server = new LoopbackServer().Start();
        _client = new XingenClient("xgn_test_abc123", _server.BaseUrl);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _server.DisposeAsync();
    }

    [Fact]
    public async Task SubmitSendsExactBackendRequestShapeAndDecodes202()
    {
        string? capturedBody = null;
        _server.MapHandler("/v1/invoices", async context =>
        {
            if (context.Request.HttpMethod != "POST")
            {
                context.Response.StatusCode = 405;
                context.Response.Close();
                return;
            }
            capturedBody = await context.ReadBodyAsync();
            await context.RespondAsync(202, "{\"id\":\"inv_123\",\"status\":\"processing\"}");
        });

        var submission = new InvoiceSubmission
        {
            InvoiceNumber = "INV-2024-0042",
            IssueDate = new DateOnly(2024, 3, 15),
            Currency = "EUR",
            BuyerReference = "991-12345-06",
            ValidationProfile = ValidationProfile.XRECHNUNG,
            Supplier = new InvoiceSubmission.PartyInput { Name = "Acme GmbH", VatId = "DE123456789" },
            Buyer = new InvoiceSubmission.PartyInput { Name = "Buyer Co", LeitwegId = "991-12345-06" },
            Lines =
            [
                new InvoiceSubmission.LineInput
                {
                    Description = "Software License Q1", Quantity = 5m, Unit = "C62", Price = 199.00m, TaxRate = 19m,
                },
            ],
        };

        var result = await _client.Invoices.SubmitAsync(submission);

        Assert.Equal("inv_123", result.Id);
        Assert.Equal(InvoiceStatus.Processing, result.Status);

        Assert.Contains("\"invoiceNumber\":\"INV-2024-0042\"", capturedBody);
        Assert.Contains("\"validationProfile\":\"XRECHNUNG\"", capturedBody);
        Assert.Contains("\"supplier\":{\"name\":\"Acme GmbH\",\"vatId\":\"DE123456789\"", capturedBody);
        Assert.Contains("\"lines\":[{\"description\":\"Software License Q1\"", capturedBody);
    }

    [Fact]
    public async Task ValidateFileSendsProfileAsQueryParamAndFileAsMultipartField()
    {
        string? capturedBody = null;
        string? capturedQuery = null;
        string? capturedContentType = null;
        _server.MapHandler(ValidatePath, async context =>
        {
            capturedQuery = context.Request.Url!.Query.TrimStart('?');
            capturedContentType = context.Request.ContentType;
            capturedBody = await context.ReadBodyAsync();
            await context.RespondAsync(202, "{\"id\":\"inv_456\",\"status\":\"processing\"}");
        });

        var result = await _client.Invoices.ValidateFileAsync(
            "invoice.xml", "<Invoice/>"u8.ToArray(), ValidationProfile.EN16931);

        Assert.Equal("inv_456", result.Id);
        Assert.Equal("profile=EN16931", capturedQuery);
        Assert.StartsWith("multipart/form-data; boundary=", capturedContentType);

        // .NET's MultipartFormDataContent doesn't quote Content-Disposition parameter values by
        // default (unlike some other HTTP stacks) — this is still RFC-2183-valid since "file" and
        // "invoice.xml" are plain tokens with no characters that require quoting.
        Assert.Contains("name=file; filename=invoice.xml", capturedBody);
        Assert.Contains("Content-Type: application/xml", capturedBody);
        Assert.Contains("<Invoice/>", capturedBody);
        // the gotcha this test guards against: profile must never be sent as a form field
        Assert.DoesNotContain("name=profile", capturedBody);
    }

    [Fact]
    public async Task GetDecodesInvoiceRecordEnvelope()
    {
        _server.MapHandler("/v1/invoices/inv_01HXYZ", context => context.RespondAsync(200, Fixture));

        var record = await _client.Invoices.GetAsync("inv_01HXYZ");

        Assert.Equal("inv_01HXYZ", record.Id);
        Assert.Equal(InvoiceStatus.Validated, record.Status);
        Assert.Equal("INV-2024-0042", record.Invoice!.InvoiceNumber);
    }

    [Fact]
    public async Task ListSendsPageSizeAndSortAsQueryParams()
    {
        string? capturedQuery = null;
        _server.MapHandler("/v1/invoices", async context =>
        {
            capturedQuery = context.Request.Url!.Query.TrimStart('?');
            await context.RespondAsync(200, SinglePage(Fixture, true));
        });

        await _client.Invoices.ListAsync(2, 10, "createdAt,desc");

        Assert.Contains("page=2", capturedQuery);
        Assert.Contains("size=10", capturedQuery);
        Assert.Contains("sort=createdAt%2Cdesc", capturedQuery);
    }

    [Fact]
    public async Task SubmitODataSendsProfileAsQueryParamAndRawJsonAsBody()
    {
        string? capturedBody = null;
        string? capturedQuery = null;
        _server.MapHandler("/v1/invoices/validate/odata", async context =>
        {
            capturedQuery = context.Request.Url!.Query.TrimStart('?');
            capturedBody = await context.ReadBodyAsync();
            await context.RespondAsync(202, "{\"id\":\"inv_odata\",\"status\":\"processing\"}");
        });

        var result = await _client.Invoices.SubmitODataAsync("{\"SupplierInvoice\":\"raw-payload\"}", ValidationProfile.EN16931);

        Assert.Equal("inv_odata", result.Id);
        Assert.Equal("profile=EN16931", capturedQuery);
        Assert.Equal("{\"SupplierInvoice\":\"raw-payload\"}", capturedBody);
    }

    [Fact]
    public async Task DownloadPdfReturnsRawBytesWithPdfAccept()
    {
        byte[] pdfBytes = [0x25, 0x50, 0x44, 0x46]; // "%PDF"
        string? capturedAccept = null;
        _server.MapHandler("/v1/invoices/inv_01HXYZ/download", async context =>
        {
            capturedAccept = context.Request.Headers["Accept"];
            await context.RespondAsync(200, pdfBytes, "application/pdf");
        });

        var result = await _client.Invoices.DownloadPdfAsync("inv_01HXYZ");

        Assert.Equal(pdfBytes, result);
        Assert.Equal("application/pdf", capturedAccept);
    }

    [Fact]
    public async Task DownloadIdocXmlReturnsRawBytesWithXmlAccept()
    {
        var xmlBytes = "<IDOC/>"u8.ToArray();
        string? capturedAccept = null;
        _server.MapHandler("/v1/invoices/inv_01HXYZ/download/idoc", async context =>
        {
            capturedAccept = context.Request.Headers["Accept"];
            await context.RespondAsync(200, xmlBytes, "application/xml");
        });

        var result = await _client.Invoices.DownloadIdocXmlAsync("inv_01HXYZ");

        Assert.Equal(xmlBytes, result);
        Assert.Equal("application/xml", capturedAccept);
    }

    [Fact]
    public async Task ListAllLazilyWalksMultiplePages()
    {
        _server.MapHandler("/v1/invoices", context =>
        {
            var isLastPage = context.Request.Url!.Query.Contains("page=1");
            return context.RespondAsync(200, SinglePage(Fixture, isLastPage));
        });

        var all = new List<InvoiceRecord>();
        await foreach (var record in _client.Invoices.ListAllAsync(1))
        {
            all.Add(record);
        }

        Assert.Equal(2, all.Count);
    }

    private static string SinglePage(string recordJson, bool last) =>
        "{\"content\":[" + recordJson + "],\"totalElements\":2,\"totalPages\":2,"
        + "\"number\":" + (last ? 1 : 0) + ",\"size\":1,\"first\":" + (!last).ToString().ToLowerInvariant() + ",\"last\":" + last.ToString().ToLowerInvariant()
        + ",\"numberOfElements\":1,\"empty\":false}";
}
