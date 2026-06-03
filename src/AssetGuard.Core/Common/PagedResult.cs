namespace AssetGuard.Core.Common;

public interface IPagedResult
{
    int TotalCount { get; }
    int Page { get; }
    int PageSize { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}

public class PagedResult<T> : IPagedResult
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public class SearchFilter
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? DeviceType { get; set; }
    public string? Department { get; set; }
    public string? ActionType { get; set; }
    public string? EntityName { get; set; }
    public string? UserName { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? DeviceId { get; set; }
    public int? EmployeeId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public static class WarrantyFilterStatus
{
    public const string Active = "Active";
    public const string Expired = "Expired";
    public const string Expiring30 = "Expiring30";
}
