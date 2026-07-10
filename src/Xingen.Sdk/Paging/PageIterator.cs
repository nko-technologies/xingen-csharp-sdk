using System.Runtime.CompilerServices;

namespace Xingen.Sdk.Paging;

/// <summary>
/// Lazily enumerates every element across all pages of a paginated endpoint, fetching the next page
/// only once the current one is exhausted, so callers can <c>await foreach</c> a whole result set
/// without managing page indices or loading everything into memory upfront.
/// </summary>
public static class PageIterator
{
    public static async IAsyncEnumerable<T> EnumerateAsync<T>(
        Func<int, CancellationToken, Task<Page<T>>> pageFetcher,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var currentPage = await pageFetcher(0, cancellationToken).ConfigureAwait(false);
        while (true)
        {
            foreach (var item in currentPage.Content)
            {
                yield return item;
            }
            if (currentPage.Last)
            {
                yield break;
            }
            currentPage = await pageFetcher(currentPage.Number + 1, cancellationToken).ConfigureAwait(false);
        }
    }
}
