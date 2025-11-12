using System.ComponentModel.DataAnnotations;

namespace CommunityCar.Api.Models;

public class PagedRequest
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    private int _pageSize = DefaultPageSize;

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, MaxPageSize)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 1 : value;
    }

    public string? SortBy { get; set; }

    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    public string? Search { get; set; }

    public Dictionary<string, object>? Filters { get; set; }

    public int Skip => (Page - 1) * PageSize;

    public int Take => PageSize;

    public bool HasSearch => !string.IsNullOrWhiteSpace(Search);

    public bool HasFilters => Filters != null && Filters.Any();

    public bool HasSorting => !string.IsNullOrWhiteSpace(SortBy);
}

public enum SortDirection
{
    Ascending,
    Descending
}

public class PagedRequest<TFilter> : PagedRequest where TFilter : class, new()
{
    public TFilter? Filter { get; set; }

    public bool HasAdvancedFilter => Filter != null;
}

public class DataTableRequest : PagedRequest
{
    public int Draw { get; set; } // DataTables draw counter

    public List<DataTableColumn>? Columns { get; set; }

    public List<DataTableOrder>? Order { get; set; }

    public DataTableSearch? Search { get; set; }

    public bool IsDataTableRequest => Draw > 0;
}

public class DataTableColumn
{
    public string Data { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Searchable { get; set; }
    public bool Orderable { get; set; }
    public DataTableSearch? Search { get; set; }
}

public class DataTableOrder
{
    public int Column { get; set; }
    public string Dir { get; set; } = "asc";
}

public class DataTableSearch
{
    public string Value { get; set; } = string.Empty;
    public bool Regex { get; set; }
}

public class CursorPagedRequest : PagedRequest
{
    public string? Cursor { get; set; }

    public bool HasCursor => !string.IsNullOrWhiteSpace(Cursor);

    public CursorDirection Direction { get; set; } = CursorDirection.Next;
}

public enum CursorDirection
{
    Next,
    Previous
}

public class InfiniteScrollRequest : PagedRequest
{
    public string? After { get; set; }
    public string? Before { get; set; }

    public bool IsLoadMore => !string.IsNullOrWhiteSpace(After);
    public bool IsLoadPrevious => !string.IsNullOrWhiteSpace(Before);
}

public class SearchRequest : PagedRequest
{
    public string? Query { get; set; }

    public List<string>? Fields { get; set; }

    public SearchOperator Operator { get; set; } = SearchOperator.And;

    public bool Fuzzy { get; set; }

    public int? MinScore { get; set; }

    public Dictionary<string, float>? FieldBoosts { get; set; }
}

public enum SearchOperator
{
    And,
    Or
}

public class ExportRequest : PagedRequest
{
    public ExportFormat Format { get; set; } = ExportFormat.CSV;

    public List<string>? Fields { get; set; }

    public string? FileName { get; set; }

    public bool IncludeHeaders { get; set; } = true;
}

public enum ExportFormat
{
    CSV,
    Excel,
    PDF,
    JSON,
    XML
}

public class BulkOperationRequest<T>
{
    [Required]
    [MinLength(1)]
    public List<T> Items { get; set; } = new();

    public bool ContinueOnError { get; set; } = false;

    public int BatchSize { get; set; } = 100;

    public Dictionary<string, object>? Options { get; set; }
}

public class ApiRequestContext
{
    public string? RequestId { get; set; }
    public string? UserId { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? Metadata { get; set; }
}