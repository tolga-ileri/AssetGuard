namespace AssetGuard.Web.ViewModels;

public class ListFilterViewModel
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? DeviceType { get; set; }
    public string? Department { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
