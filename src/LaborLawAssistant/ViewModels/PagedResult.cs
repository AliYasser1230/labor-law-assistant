namespace LaborLawAssistant.ViewModels;

public record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    public int FirstItemNumber => TotalCount == 0 ? 0 : (PageNumber - 1) * PageSize + 1;
    public int LastItemNumber => Math.Min(PageNumber * PageSize, TotalCount);

    public PaginationViewModel Pagination => new(PageNumber, TotalPages);
}

public record PaginationViewModel(int PageNumber, int TotalPages);
