using AssetGuard.Core.DTOs;

namespace AssetGuard.Core.Interfaces;

public interface IReportRepository
{
    Task<IReadOnlyList<InventoryReportRow>> GetInventoryReportAsync(ReportFilter filter);
    Task<IReadOnlyList<AssignedDeviceReportRow>> GetAssignedDevicesReportAsync(ReportFilter filter);
    Task<MaintenanceCostReportResult> GetMaintenanceCostReportAsync(ReportFilter filter);
    Task<IReadOnlyList<WarrantyExpirationReportRow>> GetWarrantyExpirationReportAsync(ReportFilter filter);
    Task<IReadOnlyList<DepartmentDeviceReportRow>> GetDepartmentDeviceReportAsync(ReportFilter filter);
}
