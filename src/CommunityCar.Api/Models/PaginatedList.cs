using System.Text.Json.Serialization;

namespace CommunityCar.Api.Models;

public class PaginatedList<T>
{
    public List<T> Items { get; set; } = new();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    public int FirstItemIndex => Items.Any() ? ((Page - 1) * PageSize) + 1 : 0;

    public int LastItemIndex => Items.Any() ? FirstItemIndex + Items.Count - 1 : 0;

    [JsonIgnore]
    public bool IsFirstPage => Page == 1;

    [JsonIgnore]
    public bool IsLastPage => Page == TotalPages;

    public PaginatedList()
    {
    }

    public PaginatedList(List<T> items, int count, int page, int pageSize)
    {
        Items = items;
        TotalCount = count;
        Page = page;
        PageSize = pageSize;
    }

    public static PaginatedList<T> Create(List<T> source, int page, int pageSize)
    {
        var count = source.Count;
        var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedList<T>(items, count, page, pageSize);
    }

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int page, int pageSize)
    {
        var count = await Task.FromResult(source.Count());
        var items = await Task.FromResult(source.Skip((page - 1) * pageSize).Take(pageSize).ToList());

        return new PaginatedList<T>(items, count, page, pageSize);
    }

    public PaginatedList<T> ToPagedResult()
    {
        return new PaginatedList<T>(Items, TotalCount, Page, PageSize);
    }
}

public class PaginatedList
{
    public static PaginatedList<T> Empty<T>() => new();

    public static PaginatedList<T> From<T>(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        return new PaginatedList<T>(items.ToList(), totalCount, page, pageSize);
    }
}

public class CursorPaginatedList<T> : PaginatedList<T>
{
    public string? NextCursor { get; set; }

    public string? PreviousCursor { get; set; }

    public bool HasNextCursor => !string.IsNullOrEmpty(NextCursor);

    public bool HasPreviousCursor => !string.IsNullOrEmpty(PreviousCursor);

    public CursorPaginatedList() : base()
    {
    }

    public CursorPaginatedList(List<T> items, string? nextCursor, string? previousCursor)
        : base(items, items.Count, 1, items.Count)
    {
        NextCursor = nextCursor;
        PreviousCursor = previousCursor;
    }
}

public class InfiniteScrollList<T> : PaginatedList<T>
{
    public string? NextPageToken { get; set; }

    public string? PreviousPageToken { get; set; }

    public bool HasMore => !string.IsNullOrEmpty(NextPageToken);

    public InfiniteScrollList() : base()
    {
    }

    public InfiniteScrollList(List<T> items, string? nextPageToken, string? previousPageToken)
        : base(items, items.Count, 1, items.Count)
    {
        NextPageToken = nextPageToken;
        PreviousPageToken = previousPageToken;
    }
}

public class PaginationMetadata
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    public int FirstItemIndex { get; set; }
    public int LastItemIndex { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public string? SearchQuery { get; set; }
    public Dictionary<string, object>? Filters { get; set; }

    public static PaginationMetadata FromPagedRequest<T>(PaginatedList<T> list, PagedRequest request)
    {
        return new PaginationMetadata
        {
            Page = list.Page,
            PageSize = list.PageSize,
            TotalCount = list.TotalCount,
            TotalPages = list.TotalPages,
            HasPreviousPage = list.HasPreviousPage,
            HasNextPage = list.HasNextPage,
            FirstItemIndex = list.FirstItemIndex,
            LastItemIndex = list.LastItemIndex,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection.ToString(),
            SearchQuery = request.Search,
            Filters = request.Filters
        };
    }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public PaginationMetadata? Pagination { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? RequestId { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponse<PaginatedList<T>> PaginatedResponse(PaginatedList<T> data, PagedRequest request, string? message = null)
    {
        return new ApiResponse<PaginatedList<T>>
        {
            Success = true,
            Data = data,
            Message = message,
            Pagination = PaginationMetadata.FromPagedRequest(data, request)
        };
    }
}

public class DataTableResponse<T> : ApiResponse<List<T>>
{
    public int Draw { get; set; }
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }

    public DataTableResponse(int draw, List<T> data, int totalCount, int filteredCount)
    {
        Draw = draw;
        Data = data;
        RecordsTotal = totalCount;
        RecordsFiltered = filteredCount;
        Success = true;
    }
}

public class SelectListItem
{
    public string? Value { get; set; }
    public string? Text { get; set; }
    public bool Selected { get; set; }
    public bool Disabled { get; set; }
    public object? Group { get; set; }
}

public class SelectList
{
    public IEnumerable<SelectListItem> Items { get; set; } = new List<SelectListItem>();

    public SelectList(IEnumerable<SelectListItem> items)
    {
        Items = items;
    }

    public static SelectList FromEnum<TEnum>() where TEnum : struct, Enum
    {
        var items = Enum.GetValues<TEnum>()
            .Select(e => new SelectListItem
            {
                Value = e.ToString(),
                Text = e.ToString()
            });

        return new SelectList(items);
    }

    public static SelectList FromDictionary(Dictionary<string, string> dictionary)
    {
        var items = dictionary.Select(kvp => new SelectListItem
        {
            Value = kvp.Key,
            Text = kvp.Value
        });

        return new SelectList(items);
    }
}