namespace StateManagement.Modules.Shared.Models;

public record PagedListOut<T>
{
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; set; }
    public required IReadOnlyList<T> Items { get; init; }
    public bool HasNextPage => TotalCount > Page * PageSize;
    public bool HasPreviousPage => Page > 1;
}
