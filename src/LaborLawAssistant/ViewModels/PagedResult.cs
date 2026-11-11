namespace LaborLawAssistant.ViewModels;

/// <summary>
/// One page of a long list: the items on this page + the numbers needed for the page buttons.
/// Example: 74 articles, 12 per page → 7 pages; page 2 shows articles 13–24.
/// </summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    // The position of the first and last item on this page (for "Showing 13–24 of 74").
    public int FirstItemNumber => TotalCount == 0 ? 0 : (PageNumber - 1) * PageSize + 1;
    public int LastItemNumber => Math.Min(PageNumber * PageSize, TotalCount);

    public PaginationViewModel Pagination => new(PageNumber, TotalPages);
}

/// <summary>What the page buttons need (Views/Shared/_Pagination.cshtml).</summary>
public record PaginationViewModel(int PageNumber, int TotalPages);
