using AssetGuard.Core.Common;
using AssetGuard.Web.ViewModels;

namespace AssetGuard.Web.Helpers;

public static class FilterMapper
{
    public static SearchFilter ToFilter(ListFilterViewModel vm) => new()
    {
        SearchTerm = vm.SearchTerm,
        Status = vm.Status,
        DeviceType = vm.DeviceType,
        Department = vm.Department,
        Page = vm.Page < 1 ? 1 : vm.Page,
        PageSize = vm.PageSize < 1 ? 10 : vm.PageSize
    };

    public static SearchFilter ToFilter(AuditLogFilterViewModel vm) => new()
    {
        SearchTerm = vm.SearchTerm,
        UserName = vm.UserName,
        ActionType = vm.ActionType,
        EntityName = vm.EntityName,
        DateFrom = vm.DateFrom,
        DateTo = vm.DateTo,
        Page = vm.Page < 1 ? 1 : vm.Page,
        PageSize = vm.PageSize < 1 ? 20 : vm.PageSize
    };
}
