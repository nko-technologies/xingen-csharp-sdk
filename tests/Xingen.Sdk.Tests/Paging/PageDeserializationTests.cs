using Xingen.Sdk.Http;
using Xingen.Sdk.Invoices;
using Xingen.Sdk.Paging;
using Xunit;

namespace Xingen.Sdk.Tests.Paging;

public class PageDeserializationTests
{
    private readonly JsonCodec _codec = new();

    [Fact]
    public async Task DecodesPageIgnoringUnknownSpringDataFields()
    {
        var bytes = await File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "page-of-invoices.json"));
        var page = _codec.Decode<Page<InvoiceRecord>>(bytes);

        Assert.Single(page.Content);
        Assert.Equal("inv_01HXYZ", page.Content[0].Id);
        Assert.Equal(1, page.TotalElements);
        Assert.True(page.First);
        Assert.True(page.Last);
        Assert.False(page.Empty);
    }
}
