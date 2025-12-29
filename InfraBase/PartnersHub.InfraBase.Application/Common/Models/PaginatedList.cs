namespace PartnersHub.InfraBase.Application.Common.Models;

/// <summary>
/// Represents a paginated list of items
/// </summary>
/// <typeparam name="T">The type of items in the list</typeparam>
public class PaginatedList<T>
{
    /// <summary>
    /// The items in the current page
    /// </summary>
    public List<T> Items { get; init; } = new();

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indicates if there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Indicates if there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Creates an empty paginated list
    /// </summary>
    public static PaginatedList<T> Empty(int pageNumber = 1, int pageSize = 10)
    {
        return new PaginatedList<T>(new List<T>(), 0, pageNumber, pageSize);
    }

    /// <summary>
    /// Creates a paginated list from items and total count
    /// </summary>
    public static PaginatedList<T> Create(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PaginatedList<T>(items.ToList(), totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedList{T}"/> class.
    /// </summary>
    /// <param name="items">The items in the paginated list.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <param name="pageNumber">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    public PaginatedList(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
