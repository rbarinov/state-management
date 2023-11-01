using Events.Modules.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Events.Data;

public static class QueryableExtensions
{
    public static async Task<PagedListOut<T>> ToPagedListAsync<T>(
        this IOrderedQueryable<T> source,
        int page,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        var totalCount = await source.CountAsync(cancellationToken: cancellationToken);

        var items = await source.Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return new PagedListOut<T>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }
}
