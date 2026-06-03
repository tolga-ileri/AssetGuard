using Dapper;
using AssetGuard.Core.Common;
using AssetGuard.Core.DTOs;
using AssetGuard.Core.Interfaces;
using AssetGuard.Core.Models;

namespace AssetGuard.Infrastructure.Repositories;

public class MaintenanceRepository : IMaintenanceRepository
{
    private readonly IDapperExecutor _db;

    private const string SelectSql = """
        SELECT m.*, d.DeviceName, d.AssetTag
        FROM MaintenanceRecords m INNER JOIN Devices d ON m.DeviceId = d.Id
        """;

    public MaintenanceRepository(IDapperExecutor db) => _db = db;

    public async Task<PagedResult<MaintenanceRecord>> GetPagedAsync(SearchFilter filter)
    {
        var where = BuildWhereClause(filter, out var parameters);
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

        var (total, items) = await _db.QueryPagedAsync<MaintenanceRecord>(
            $"SELECT COUNT(*) FROM MaintenanceRecords m INNER JOIN Devices d ON m.DeviceId = d.Id {where}",
            $"""
            {SelectSql} {where}
            ORDER BY m.MaintenanceDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """,
            parameters);

        return new PagedResult<MaintenanceRecord> { Items = items, TotalCount = total, Page = filter.Page, PageSize = filter.PageSize };
    }

    public Task<MaintenanceRecord?> GetByIdAsync(int id) =>
        _db.QueryFirstOrDefaultAsync<MaintenanceRecord>($"{SelectSql} WHERE m.Id = @Id", new { Id = id });

    public Task<int> CreateAsync(MaintenanceRecord record) =>
        _db.ExecuteScalarAsync<int>("""
            INSERT INTO MaintenanceRecords (DeviceId, MaintenanceDate, MaintenanceType, Description, Cost, PerformedBy, NextMaintenanceDate)
            OUTPUT INSERTED.Id
            VALUES (@DeviceId, @MaintenanceDate, @MaintenanceType, @Description, @Cost, @PerformedBy, @NextMaintenanceDate)
            """, record);

    public Task UpdateAsync(MaintenanceRecord record) =>
        _db.ExecuteAsync("""
            UPDATE MaintenanceRecords SET DeviceId=@DeviceId, MaintenanceDate=@MaintenanceDate, MaintenanceType=@MaintenanceType,
            Description=@Description, Cost=@Cost, PerformedBy=@PerformedBy, NextMaintenanceDate=@NextMaintenanceDate WHERE Id=@Id
            """, record);

    public Task DeleteAsync(int id) =>
        _db.ExecuteAsync("DELETE FROM MaintenanceRecords WHERE Id = @Id", new { Id = id });

    public Task<decimal> GetTotalCostAsync() =>
        _db.ExecuteScalarAsync<decimal>("SELECT ISNULL(SUM(Cost), 0) FROM MaintenanceRecords");

    public Task<int> GetUpcomingCountAsync(int daysAhead = 30) =>
        _db.ExecuteScalarAsync<int>("""
            SELECT COUNT(*) FROM MaintenanceRecords
            WHERE NextMaintenanceDate IS NOT NULL AND NextMaintenanceDate <= DATEADD(DAY, @Days, GETUTCDATE())
              AND NextMaintenanceDate >= CAST(GETUTCDATE() AS DATE)
            """, new { Days = daysAhead });

    public async Task<IReadOnlyList<MaintenanceRecord>> GetUpcomingAsync(int daysAhead = 30) =>
        await _db.QueryAsync<MaintenanceRecord>($"""
            {SelectSql}
            WHERE m.NextMaintenanceDate IS NOT NULL
              AND m.NextMaintenanceDate <= DATEADD(DAY, @Days, GETUTCDATE())
              AND m.NextMaintenanceDate >= CAST(GETUTCDATE() AS DATE)
            ORDER BY m.NextMaintenanceDate ASC
            """, new { Days = daysAhead });

    public async Task<IReadOnlyList<MaintenanceRecord>> GetByDeviceIdAsync(int deviceId) =>
        await _db.QueryAsync<MaintenanceRecord>($"{SelectSql} WHERE m.DeviceId = @DeviceId ORDER BY m.MaintenanceDate DESC", new { DeviceId = deviceId });

    public async Task<IReadOnlyList<MonthlyCostPoint>> GetMonthlyCostsAsync(int months = 6) =>
        await _db.QueryAsync<MonthlyCostPoint>("""
            SELECT FORMAT(DATEFROMPARTS(YEAR(MaintenanceDate), MONTH(MaintenanceDate), 1), 'yyyy-MM') AS Month,
                   SUM(Cost) AS Amount
            FROM MaintenanceRecords
            WHERE MaintenanceDate >= DATEADD(MONTH, -@Months, DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1))
            GROUP BY YEAR(MaintenanceDate), MONTH(MaintenanceDate)
            ORDER BY YEAR(MaintenanceDate), MONTH(MaintenanceDate)
            """, new { Months = months - 1 });

    private static string BuildWhereClause(SearchFilter filter, out DynamicParameters parameters)
    {
        var where = "WHERE 1=1";
        parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            where += " AND (d.DeviceName LIKE @Search OR d.AssetTag LIKE @Search OR m.MaintenanceType LIKE @Search OR m.PerformedBy LIKE @Search)";
            parameters.Add("Search", $"%{filter.SearchTerm.Trim()}%");
        }
        if (filter.DeviceId.HasValue)
        {
            where += " AND m.DeviceId = @DeviceId";
            parameters.Add("DeviceId", filter.DeviceId.Value);
        }

        return where;
    }
}
