using System.Net;
using System.Text;
using Xingen.Sdk.Errors;
using Xingen.Sdk.Http;
using Xunit;

namespace Xingen.Sdk.Tests.Http;

public class ResponseHandlerTests
{
    private readonly JsonCodec _codec = new();

    [Fact]
    public async Task SucceedsSilentlyOn2xx()
    {
        await ResponseHandler.RequireSuccessAsync(Response(202, "{\"id\":\"abc\"}"), _codec);
    }

    [Fact]
    public async Task MapsQuotaExceededShapeThatDiffersFromErrorResponse()
    {
        var response = Response(429, "{\"error\":\"Quota exceeded\"}");

        var ex = await Assert.ThrowsAsync<QuotaExceededException>(
            () => ResponseHandler.RequireSuccessAsync(response, _codec));

        Assert.Equal("Quota exceeded", ex.Message);
        Assert.Equal(429, ex.StatusCode);
        Assert.Null(ex.ErrorResponse);
        Assert.Contains("Quota exceeded", ex.RawBody);
    }

    [Fact]
    public async Task MapsAuthenticationExceptionWithoutAttemptingErrorResponseParse()
    {
        var response = Response(401, "");
        await Assert.ThrowsAsync<AuthenticationException>(() => ResponseHandler.RequireSuccessAsync(response, _codec));
    }

    [Fact]
    public async Task MapsAuthenticationExceptionEvenWithUnexpectedHtmlBody()
    {
        var response = Response(401, "<html>not json</html>");
        await Assert.ThrowsAsync<AuthenticationException>(() => ResponseHandler.RequireSuccessAsync(response, _codec));
    }

    [Fact]
    public async Task MapsForbiddenWithErrorResponseBody()
    {
        var response = Response(403,
            "{\"message\":\"Invoice exists but is not owned by caller\",\"error\":\"FORBIDDEN\",\"code\":403,\"timestamp\":\"2026-07-08T00:00:00Z\"}");

        var ex = await Assert.ThrowsAsync<PermissionException>(() => ResponseHandler.RequireSuccessAsync(response, _codec));
        Assert.Equal("Invoice exists but is not owned by caller", ex.Message);
    }

    [Fact]
    public async Task MapsNotFound()
    {
        var response = Response(404,
            "{\"message\":\"The requested resource was not found\",\"error\":\"NOT_FOUND\",\"code\":404,\"timestamp\":\"2026-07-08T00:00:00Z\"}");

        await Assert.ThrowsAsync<NotFoundException>(() => ResponseHandler.RequireSuccessAsync(response, _codec));
    }

    [Fact]
    public async Task MapsBadRequestAndSurfacesFieldErrors()
    {
        var response = Response(400,
            "{\"message\":\"Validation failed\",\"error\":\"BAD_REQUEST\",\"code\":400,"
            + "\"timestamp\":\"2026-07-08T00:00:00Z\",\"fieldErrors\":{\"invoiceNumber\":\"must not be blank\"}}");

        var ex = await Assert.ThrowsAsync<ValidationRequestException>(() => ResponseHandler.RequireSuccessAsync(response, _codec));
        Assert.Equal("must not be blank", ex.FieldErrors["invoiceNumber"]);
    }

    [Fact]
    public async Task MapsUnmappedStatusToGenericApiExceptionWithoutThrowingOnMalformedBody()
    {
        var response = Response(500, "not even json {{{");

        var ex = await Assert.ThrowsAsync<ApiException>(() => ResponseHandler.RequireSuccessAsync(response, _codec));
        Assert.Equal(500, ex.StatusCode);
        Assert.Null(ex.ErrorResponse);
        Assert.Equal("not even json {{{", ex.RawBody);
    }

    private static HttpResponseMessage Response(int statusCode, string body) => new((HttpStatusCode)statusCode)
    {
        Content = new ByteArrayContent(Encoding.UTF8.GetBytes(body)),
    };
}
