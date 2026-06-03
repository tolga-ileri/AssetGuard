using Dapper;
using AssetGuard.Core.DTOs;
using AssetGuard.Core.Interfaces;

namespace AssetGuard.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly IDapperExecutor _db;

    public ReportRepository(IDapperExecutor db) => _db = db;

    public async Task<IReadOnlyList<InventoryReportRow>> GetInventoryReportAsync(ReportFilter filter)
    {
        var where = BuildDeviceWhere(filter, out var p);
        if (filter.DateFrom.HasValue) { where += " AND PurchaseDate >= @DateFrom"; p.Add("DateFrom", filter.DateFrom.Value.Date); }
        if (filter.DateTo.HasValue) { where += " AND PurchaseDate <= @DateTo"; p.Add("DateTo", filter.DateTo.Value.Date); }

        return await _db.QueryAsync<InventoryReportRow>($"""
            SELECT AssetTag, SerialNumber, DeviceName, DeviceType, Brand, Model, Status, Location, PurchaseDate, WarrantyEndDate
            FROM Devices {where} ORDER BY AssetTag
            """, p);
    }

    public async Task<IReadOnlyList<AssignedDeviceReportRow>> GetAssignedDevicesReportAsync(ReportFilter filter)
    {
        var where = "WHERE a.Status = 'Active'";
        var p = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(filter.Department)) { where += " AND e.Department = @Department"; p.Add("Department", filter.Department); }
        if (!string.IsNullOrWhiteSpace(filter.DeviceType)) { where += " AND d.DeviceType = @DeviceType"; p.Add("DeviceType", filter.DeviceType); }
        if (!string.IsNullOrWhiteSpace(filter.Status)) { where += " AND d.Status = @Status"; p.Add("Status", filter.Status); }
        if (filter.DateFrom.HasValue) { where += " AND a.AssignedDate >= @DateFrom"; p.Add("DateFrom", filter.DateFrom.Value.Date); }
        if (filter.DateTo.HasValue) { where += " AND a.AssignedDate <= @DateTo"; p.Add("DateTo", filter.DateTo.Value.Date); }

        return await _db.QueryAsync<AssignedDeviceReportRow>($"""
            SELECT d.AssetTag, d.DeviceName, d.DeviceType, e.FullName AS EmployeeName, e.Department,
                   a.AssignedDate, a.Status, a.AssignmentNote
            FROM Assignments a
            INNER JOIN Devices d ON a.DeviceId = d.Id
            INNER JOIN Employees e ON a.EmployeeId = e.Id
            {where} ORDER BY e.Department, e.FullName
            """, p);
    }

    public async Task<MaintenanceCostReportResult> GetMaintenanceCostReportAsync(ReportFilter filter)
    {
        var where = "WHERE 1=1";
        var p = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(filter.DeviceType)) { where += " AND d.DeviceType = @DeviceType"; p.Add("DeviceType", filter.DeviceType); }
        if (filter.DateFrom.HasValue) { where += " AND m.MaintenanceDate >= @DateFrom"; p.Add("DateFrom", filter.DateFrom.Value.Date); }
        if (filter.DateTo.HasValue) { where += " AND m.MaintenanceDate <= @DateTo"; p.Add("DateTo", filter.DateTo.Value.Date); }

        var rows = await _db.QueryAsync<MaintenanceCostReportRow>($"""
            SELECT m.MaintenanceDate, d.AssetTag, d.DeviceName, d.DeviceType, m.MaintenanceType,
                   m.Description, m.Cost, m.PerformedBy
            FROM MaintenanceRecords m INNER JOIN Devices d ON m.DeviceId = d.Id
            {where} ORDER BY m.MaintenanceDate DESC
            """, p);

        return new MaintenanceCostReportResult
        {
            Rows = rows,
            TotalCost = rows.Sum(r => r.Cost),
            RecordCount = rows.Count
        };
    }

    public async Task<IReadOnlyList<WarrantyExpirationReportRow>> GetWarrantyExpirationReportAsync(ReportFilter filter)
    {
        var where = "WHERE 1=1";
        var p = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(filter.DeviceType)) { where += " AND d.DeviceType = @DeviceType"; p.Add("DeviceType", filter.DeviceType); }
        if (filter.DateFrom.HasValue) { where += " AND w.EndDate >= @DateFrom"; p.Add("DateFrom", filter.DateFrom.Value.Date); }
        if (filter.DateTo.HasValue) { where += " AND w.EndDate <= @DateTo"; p.Add("DateTo", filter.DateTo.Value.Date); }
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            where += filter.Status switch
            {
                "Expired" => " AND (w.EndDate < CAST(GETUTCDATE() AS DATE) OR w.IsActive = 0)",
                "Expiring30" => " AND w.IsActive = 1 AND w.EndDate >= CAST(GETUTCDATE() AS DATE) AND w.EndDate <= DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE))",
                "Active" => " AND w.IsActive = 1 AND w.EndDate > DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE))",
                _ => ""
            };
        }

        return await _db.QueryAsync<WarrantyExpirationReportRow>($"""
            SELECT d.AssetTag, d.DeviceName, d.DeviceType, w.Provider, w.StartDate, w.EndDate,
                   DATEDIFF(DAY, CAST(GETUTCDATE() AS DATE), w.EndDate) AS DaysRemaining,
                   CASE
                       WHEN w.EndDate < CAST(GETUTCDATE() AS DATE) OR w.IsActive = 0 THEN 'Expired'
                       WHEN w.EndDate <= DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE)) THEN 'Expiring Soon'
                       ELSE 'Active'
                   END AS WarrantyStatus
            FROM Warranties w INNER JOIN Devices d ON w.DeviceId = d.Id
            {where} ORDER BY w.EndDate ASC
            """, p);
    }

    public async Task<IReadOnlyList<DepartmentDeviceReportRow>> GetDepartmentDeviceReportAsync(ReportFilter filter)
    {
        var where = "WHERE a.Status = 'Active'";
        var p = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(filter.Department)) { where += " AND e.Department = @Department"; p.Add("Department", filter.Department); }
        if (!string.IsNullOrWhiteSpace(filter.DeviceType)) { where += " AND d.DeviceType = @DeviceType"; p.Add("DeviceType", filter.DeviceType); }
        if (!string.IsNullOrWhiteSpace(filter.Status)) { where += " AND d.Status = @Status"; p.Add("Status", filter.Status); }
        if (filter.DateFrom.HasValue) { where += " AND a.AssignedDate >= @DateFrom"; p.Add("DateFrom", filter.DateFrom.Value.Date); }
        if (filter.DateTo.HasValue) { where += " AND a.AssignedDate <= @DateTo"; p.Add("DateTo", filter.DateTo.Value.Date); }

        return await _db.QueryAsync<DepartmentDeviceReportRow>($"""
            SELECT e.Department, e.FullName AS EmployeeName, d.AssetTag, d.DeviceName, d.DeviceType,
                   a.AssignedDate, d.Status AS DeviceStatus
            FROM Assignments a
            INNER JOIN Devices d ON a.DeviceId = d.Id
            INNER JOIN Employees e ON a.EmployeeId = e.Id
            {where} ORDER BY e.Department, e.FullName, d.AssetTag
            """, p);
    }

    private static string BuildDeviceWhere(ReportFilter filter, out DynamicParameters parameters)
    {
        var where = "WHERE 1=1";
        parameters = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(filter.Status)) { where += " AND Status = @Status"; parameters.Add("Status", filter.Status); }
        if (!string.IsNullOrWhiteSpace(filter.DeviceType)) { where += " AND DeviceType = @DeviceType"; parameters.Add("DeviceType", filter.DeviceType); }
        return where;
    }
}
