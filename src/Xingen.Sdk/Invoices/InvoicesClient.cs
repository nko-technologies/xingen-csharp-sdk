using System.Net.Http.Headers;
using System.Text;
using Xingen.Sdk.Errors;
using Xingen.Sdk.Http;
using Xingen.Sdk.Models;
using Xingen.Sdk.Paging;

namespace Xingen.Sdk.Invoices;

/// <summary>Submit invoices for validation and retrieve results. Reachable via <see cref="XingenClient.Invoices"/>.</summary>
public sealed class InvoicesClient
{
    private const string BasePath = "/v1/invoices";
    private const string ValidatePath = BasePath + "/validate";
    private const string ValidateIdocPath = BasePath + "/validate/idoc";
    private const string ValidateOdataPath = BasePath + "/validate/odata";
    private const string ExtractPath = BasePath + "/extract";
    private const string AutoFilledFieldsPath = BasePath + "/auto-filled-fields";

    private readonly IHttpTransport _transport;
    private readonly RequestBuilder _requestBuilder;
    private readonly JsonCodec _json;

    public InvoicesClient(IHttpTransport transport, RequestBuilder requestBuilder, JsonCodec json)
    {
        _transport = transport;
        _requestBuilder = requestBuilder;
        _json = json;
    }

    /// <summary>
    /// Queues a structured JSON invoice for async validation. Poll <see cref="GetAsync"/> or use
    /// <see cref="SubmitAndWaitAsync"/> for the result.
    /// </summary>
    public Task<InvoiceSubmissionResult> SubmitAsync(InvoiceSubmission submission, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(submission);
        return DecodeAsync<InvoiceSubmissionResult>(JsonPost(BasePath, submission), cancellationToken);
    }

    public async Task<InvoiceRecord> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var request = _requestBuilder.NewRequest($"{BasePath}/{id}");
        return await DecodeAsync<InvoiceRecord>(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Matches the backend's default sort of <c>createdAt,desc</c> when <paramref name="sort"/> is null.</summary>
    public async Task<Page<InvoiceRecord>> ListAsync(
        int page, int size, string? sort = null, CancellationToken cancellationToken = default)
    {
        var query = RequestBuilder.Query().Put("page", page).Put("size", size).Put("sort", sort);
        var request = _requestBuilder.NewRequest(BasePath, query);
        var response = await Requests.SendAsync(_transport, request, cancellationToken).ConfigureAwait(false);
        return await ResponseHandler.DecodeOrThrowAsync<Page<InvoiceRecord>>(response, _json).ConfigureAwait(false);
    }

    /// <summary>Lazily enumerates every invoice across all pages, fetching the next page only once the current one is exhausted.</summary>
    public IAsyncEnumerable<InvoiceRecord> ListAllAsync(int pageSize, CancellationToken cancellationToken = default) =>
        PageIterator.EnumerateAsync<InvoiceRecord>(
            (pageIndex, ct) => ListAsync(pageIndex, pageSize, "createdAt,desc", ct),
            cancellationToken);

    /// <summary>
    /// Applies a JSON merge-patch (RFC 7386) to the invoice's canonical fields — e.g. to fill in
    /// fields an AI extraction missed, or fix a value flagged by validation — and re-validates
    /// synchronously. Array fields (lines, paymentMeans, allowanceCharges, taxBreakdowns) are
    /// replaced wholesale when present in the patch; submit the complete corrected array, not a
    /// single element. Only invoices that have finished processing can be corrected.
    /// </summary>
    public Task<InvoiceRecord> PatchInvoiceAsync(
        string id, Dictionary<string, object?> patch, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patch);
        return PatchAsync(id, _json.Encode(patch), cancellationToken);
    }

    /// <summary>Same as the dictionary overload, for callers who already hold the merge-patch as raw JSON.</summary>
    public Task<InvoiceRecord> PatchInvoiceAsync(
        string id, string rawJsonPatch, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawJsonPatch);
        return PatchAsync(id, Encoding.UTF8.GetBytes(rawJsonPatch), cancellationToken);
    }

    private Task<InvoiceRecord> PatchAsync(string id, byte[] body, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var request = _requestBuilder.NewRequest($"{BasePath}/{id}");
        request.Method = HttpMethod.Patch;
        request.Content = new ByteArrayContent(body);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return DecodeAsync<InvoiceRecord>(request, cancellationToken);
    }

    /// <summary>
    /// Returns, per validation profile, the invoice fields the JSON create/PATCH/AI-extraction
    /// endpoints backfill automatically (e.g. invoice type code, specification identifier) — so a
    /// client can tell the user rather than leave the gap unexplained.
    /// </summary>
    public async Task<Dictionary<string, List<AutoFilledField>>> GetAutoFilledFieldsAsync(
        CancellationToken cancellationToken = default)
    {
        var request = _requestBuilder.NewRequest(AutoFilledFieldsPath);
        var response = await Requests.SendAsync(_transport, request, cancellationToken).ConfigureAwait(false);
        return await ResponseHandler.DecodeOrThrowAsync<Dictionary<string, List<AutoFilledField>>>(response, _json)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Uploads a UBL XML, CII XML, or ZUGFeRD PDF file for validation. Processing is asynchronous —
    /// poll <see cref="GetAsync"/> or use <see cref="ValidateFileAndWaitAsync(string,ValidationProfile,PollOptions?,CancellationToken)"/> for the result.
    /// </summary>
    public async Task<InvoiceSubmissionResult> ValidateFileAsync(
        string filePath, ValidationProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var content = await File.ReadAllBytesAsync(filePath, cancellationToken).ConfigureAwait(false);
        return await ValidateAsync(ValidatePath, Path.GetFileName(filePath), content, profile, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Same as the file-path overload, for callers who already hold the file bytes in memory.</summary>
    public Task<InvoiceSubmissionResult> ValidateFileAsync(
        string filename, byte[] content, ValidationProfile profile, CancellationToken cancellationToken = default) =>
        ValidateAsync(ValidatePath, filename, content, profile, cancellationToken);

    /// <summary>Uploads a SAP IDoc XML file for validation. Processing is asynchronous — poll <see cref="GetAsync"/> or use <see cref="ValidateIdocAndWaitAsync(string,ValidationProfile,PollOptions?,CancellationToken)"/>.</summary>
    public async Task<InvoiceSubmissionResult> ValidateIdocAsync(
        string filePath, ValidationProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var content = await File.ReadAllBytesAsync(filePath, cancellationToken).ConfigureAwait(false);
        return await ValidateAsync(ValidateIdocPath, Path.GetFileName(filePath), content, profile, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Same as the file-path overload, for callers who already hold the file bytes in memory.</summary>
    public Task<InvoiceSubmissionResult> ValidateIdocAsync(
        string filename, byte[] content, ValidationProfile profile, CancellationToken cancellationToken = default) =>
        ValidateAsync(ValidateIdocPath, filename, content, profile, cancellationToken);

    /// <summary>
    /// Submits a raw SAP S/4HANA OData supplier-invoice JSON payload for validation. Ships as a thin
    /// passthrough in v1 rather than a fully typed model — the payload is large and SAP-integration
    /// specific; a typed model is a candidate for a later release based on demand.
    /// </summary>
    public Task<InvoiceSubmissionResult> SubmitODataAsync(
        string rawJson, ValidationProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawJson);
        return DecodeAsync<InvoiceSubmissionResult>(OdataPost(Encoding.UTF8.GetBytes(rawJson), profile), cancellationToken);
    }

    /// <summary>Same as the raw-JSON overload, for callers building the payload as a dictionary instead of raw JSON.</summary>
    public Task<InvoiceSubmissionResult> SubmitODataAsync(
        Dictionary<string, object> payload, ValidationProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return DecodeAsync<InvoiceSubmissionResult>(OdataPost(_json.Encode(payload), profile), cancellationToken);
    }

    /// <summary>
    /// Uploads a plain (non-XRechnung/UBL/CII) invoice PDF — including scanned/image-based PDFs —
    /// for AI-based field extraction. The extracted invoice is validated like any other submission.
    /// Processing is asynchronous — poll <see cref="GetAsync"/> or use
    /// <see cref="ExtractInvoiceAndWaitAsync(string,ValidationProfile,ExtractionModelTier,PollOptions?,CancellationToken)"/>.
    /// <paramref name="tier"/> == <see cref="ExtractionModelTier.ACCURATE"/> requires a Pro subscription.
    /// </summary>
    public async Task<InvoiceSubmissionResult> ExtractInvoiceAsync(
        string filePath, ValidationProfile profile, ExtractionModelTier tier, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        var content = await File.ReadAllBytesAsync(filePath, cancellationToken).ConfigureAwait(false);
        return await ExtractAsync(Path.GetFileName(filePath), content, profile, tier, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Same as the file-path overload, for callers who already hold the file bytes in memory.</summary>
    public Task<InvoiceSubmissionResult> ExtractInvoiceAsync(
        string filename, byte[] content, ValidationProfile profile, ExtractionModelTier tier,
        CancellationToken cancellationToken = default) =>
        ExtractAsync(filename, content, profile, tier, cancellationToken);

    private async Task<InvoiceSubmissionResult> ExtractAsync(
        string filename, byte[] content, ValidationProfile profile, ExtractionModelTier tier,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);
        ArgumentNullException.ThrowIfNull(content);

        // profile/tier are query parameters here, not form fields, matching every other multipart endpoint.
        using var multipart = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(GuessContentType(filename));
        multipart.Add(fileContent, "file", filename);

        var query = RequestBuilder.Query().Put("profile", profile.ToString()).Put("tier", tier.ToString());
        var request = _requestBuilder.NewRequest(ExtractPath, query);
        request.Method = HttpMethod.Post;
        request.Content = multipart;

        return await DecodeAsync<InvoiceSubmissionResult>(request, cancellationToken).ConfigureAwait(false);
    }

    private HttpRequestMessage OdataPost(byte[] body, ValidationProfile profile)
    {
        var query = RequestBuilder.Query().Put("profile", profile.ToString());
        var request = _requestBuilder.NewRequest(ValidateOdataPath, query);
        request.Method = HttpMethod.Post;
        request.Content = new ByteArrayContent(body);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return request;
    }

    /// <summary>Exports a validated invoice as a ZUGFeRD-compliant PDF with embedded XML.</summary>
    public async Task<byte[]> DownloadPdfAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var request = _requestBuilder.NewRequest($"{BasePath}/{id}/download");
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/pdf"));
        var response = await Requests.SendAsync(_transport, request, cancellationToken).ConfigureAwait(false);
        return await ResponseHandler.BytesOrThrowAsync(response, _json).ConfigureAwait(false);
    }

    /// <summary>Exports a validated invoice as a SAP IDoc XML file.</summary>
    public async Task<byte[]> DownloadIdocXmlAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var request = _requestBuilder.NewRequest($"{BasePath}/{id}/download/idoc");
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        var response = await Requests.SendAsync(_transport, request, cancellationToken).ConfigureAwait(false);
        return await ResponseHandler.BytesOrThrowAsync(response, _json).ConfigureAwait(false);
    }

    /// <summary>Submits <paramref name="submission"/> and polls <see cref="GetAsync"/> until validation reaches a terminal status.</summary>
    public async Task<InvoiceRecord> SubmitAndWaitAsync(
        InvoiceSubmission submission, PollOptions? options = null, CancellationToken cancellationToken = default)
    {
        var result = await SubmitAsync(submission, cancellationToken).ConfigureAwait(false);
        return await PollUntilTerminalAsync(result.Id!, options ?? PollOptions.Default, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Uploads the file and polls <see cref="GetAsync"/> until validation reaches a terminal status.</summary>
    public async Task<InvoiceRecord> ValidateFileAndWaitAsync(
        string filePath, ValidationProfile profile, PollOptions? options = null, CancellationToken cancellationToken = default)
    {
        var result = await ValidateFileAsync(filePath, profile, cancellationToken).ConfigureAwait(false);
        return await PollUntilTerminalAsync(result.Id!, options ?? PollOptions.Default, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Same as the file-path overload, for in-memory file bytes.</summary>
    public async Task<InvoiceRecord> ValidateFileAndWaitAsync(
        string filename, byte[] content, ValidationProfile profile, PollOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ValidateFileAsync(filename, content, profile, cancellationToken).ConfigureAwait(false);
        return await PollUntilTerminalAsync(result.Id!, options ?? PollOptions.Default, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Uploads the file as a SAP IDoc and polls <see cref="GetAsync"/> until validation reaches a terminal status.</summary>
    public async Task<InvoiceRecord> ValidateIdocAndWaitAsync(
        string filePath, ValidationProfile profile, PollOptions? options = null, CancellationToken cancellationToken = default)
    {
        var result = await ValidateIdocAsync(filePath, profile, cancellationToken).ConfigureAwait(false);
        return await PollUntilTerminalAsync(result.Id!, options ?? PollOptions.Default, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Same as the file-path overload, for in-memory file bytes.</summary>
    public async Task<InvoiceRecord> ValidateIdocAndWaitAsync(
        string filename, byte[] content, ValidationProfile profile, PollOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ValidateIdocAsync(filename, content, profile, cancellationToken).ConfigureAwait(false);
        return await PollUntilTerminalAsync(result.Id!, options ?? PollOptions.Default, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Uploads the file for AI extraction and polls <see cref="GetAsync"/> until validation reaches a terminal status.</summary>
    public async Task<InvoiceRecord> ExtractInvoiceAndWaitAsync(
        string filePath, ValidationProfile profile, ExtractionModelTier tier, PollOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ExtractInvoiceAsync(filePath, profile, tier, cancellationToken).ConfigureAwait(false);
        return await PollUntilTerminalAsync(result.Id!, options ?? PollOptions.Default, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Same as the file-path overload, for in-memory file bytes.</summary>
    public async Task<InvoiceRecord> ExtractInvoiceAndWaitAsync(
        string filename, byte[] content, ValidationProfile profile, ExtractionModelTier tier, PollOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = await ExtractInvoiceAsync(filename, content, profile, tier, cancellationToken).ConfigureAwait(false);
        return await PollUntilTerminalAsync(result.Id!, options ?? PollOptions.Default, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Polls <c>GET /v1/invoices/{id}</c> with exponential backoff until the invoice reaches
    /// <see cref="InvoiceStatus.Validated"/> or <see cref="InvoiceStatus.FailedValidation"/> — both are
    /// terminal, successful outcomes from the SDK's perspective (a failed validation is still a
    /// completed API call), so only a timeout, cancellation, or transport failure throws.
    /// </summary>
    private async Task<InvoiceRecord> PollUntilTerminalAsync(string id, PollOptions options, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow + options.Timeout;
        var interval = options.InitialInterval;
        var latest = await GetAsync(id, cancellationToken).ConfigureAwait(false);

        while (!latest.Status.IsTerminal())
        {
            if (DateTimeOffset.UtcNow > deadline)
            {
                throw new XingenTimeoutException($"Timed out waiting for invoice {id} to reach a terminal status", latest);
            }
            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
            var nextMillis = Math.Min(interval.TotalMilliseconds * options.BackoffMultiplier, options.MaxInterval.TotalMilliseconds);
            interval = TimeSpan.FromMilliseconds(nextMillis);
            latest = await GetAsync(id, cancellationToken).ConfigureAwait(false);
        }
        return latest;
    }

    private async Task<InvoiceSubmissionResult> ValidateAsync(
        string path, string filename, byte[] content, ValidationProfile profile, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);
        ArgumentNullException.ThrowIfNull(content);

        // `profile` is a query parameter here, not a form field, even though the endpoint is
        // multipart/form-data — the backend binds it from the query string.
        using var multipart = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(GuessContentType(filename));
        multipart.Add(fileContent, "file", filename);

        var query = RequestBuilder.Query().Put("profile", profile.ToString());
        var request = _requestBuilder.NewRequest(path, query);
        request.Method = HttpMethod.Post;
        request.Content = multipart;

        return await DecodeAsync<InvoiceSubmissionResult>(request, cancellationToken).ConfigureAwait(false);
    }

    private static string GuessContentType(string filename)
    {
        var lower = filename.ToLowerInvariant();
        if (lower.EndsWith(".xml", StringComparison.Ordinal))
        {
            return "application/xml";
        }
        if (lower.EndsWith(".pdf", StringComparison.Ordinal))
        {
            return "application/pdf";
        }
        return "application/octet-stream";
    }

    private HttpRequestMessage JsonPost(string path, object body)
    {
        var request = _requestBuilder.NewRequest(path);
        request.Method = HttpMethod.Post;
        request.Content = new ByteArrayContent(_json.Encode(body));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return request;
    }

    private async Task<T> DecodeAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await Requests.SendAsync(_transport, request, cancellationToken).ConfigureAwait(false);
        return await ResponseHandler.DecodeOrThrowAsync<T>(response, _json).ConfigureAwait(false);
    }
}
