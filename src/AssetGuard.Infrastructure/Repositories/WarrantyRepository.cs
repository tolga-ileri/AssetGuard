using Dapper;
using AssetGuard.Core.Common;
using AssetGuard.Core.DTOs;
using AssetGuard.Core.Interfaces;
using AssetGuard.Core.Models;

namespace AssetGuard.Infrastructure.Repositories;

public class WarrantyRepository : IWarrantyRepository
{
    private readonly IDapperExecutor _db;

    private const string SelectSql = """
        SELECT w.*, d.DeviceName, d.AssetTag, d.DeviceType
        FROM Warranties w INNER JOIN Devices d ON w.DeviceId = d.Id
        """;

    public WarrantyRepository(IDapperExecutor db) => _db = db;

    public async Task<PagedResult<Warranty>> GetPagedAsync(SearchFilter filter)
    {
        var where = BuildWhereClause(filter, out var parameters);
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

        var (total, items) = await _db.QueryPagedAsync<Warranty>(
            $"SELECT COUNT(*) FROM Warranties w INNER JOIN Devices d ON w.DeviceId = d.Id {where}",
            $"""
            {SelectSql} {where}
            ORDER BY w.EndDate ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """,
            parameters);

        return new PagedResult<Warranty> { Items = items, TotalCount = total, Page = filter.Page, PageSize = filter.PageSize };
    }

    public Task<Warranty?> GetByIdAsync(int id) =>
        _db.QueryFirstOrDefaultAsync<Warranty>($"{SelectSql} WHERE w.Id = @Id", new { Id = id });

    public Task<int> CreateAsync(Warranty warranty) =>
        _db.ExecuteScalarAsync<int>("""
            INSERT INTO Warranties (DeviceId, Provider, StartDate, EndDate, CoverageDetails, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@DeviceId, @Provider, @StartDate, @EndDate, @CoverageDetails, @IsActive)
            """, warranty);

    public Task UpdateAsync(Warranty warranty) =>
        _db.ExecuteAsync("""
            UPDATE Warranties SET DeviceId=@DeviceId, Provider=@Provider, StartDate=@StartDate,
            EndDate=@EndDate, CoverageDetails=@CoverageDetails, IsActive=@IsActive WHERE Id=@Id
            """, warranty);

    public Task DeleteAsync(int id) =>
        _db.ExecuteAsync("DELETE FROM Warranties WHERE Id = @Id", new { Id = id });

    public Task<int> GetExpiringCountAsync(int daysAhead = 30) =>
        _db.ExecuteScalarAsync<int>("""
            SELECT COUNT(*) FROM Warranties
            WHERE IsActive = 1 AND EndDate <= DATEADD(DAY, @Days, CAST(GETUTCDATE() AS DATE))
              AND EndDate >= CAST(GETUTCDATE() AS DATE)
            """, new { Days = daysAhead });

    public Task<int> GetExpiredCountAsync() =>
        _db.ExecuteScalarAsync<int>("""
            SELECT COUNT(*) FROM Warranties
            WHERE EndDate < CAST(GETUTCDATE() AS DATE) OR IsActive = 0
            """);

    public async Task<WarrantySummaryCounts> GetSummaryCountsAsync()
    {
        const string sql = """
            SELECT
                SUM(CASE WHEN w.IsActive = 1 AND w.EndDate > DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE)) THEN 1 ELSE 0 END) AS Active,
                SUM(CASE WHEN w.EndDate < CAST(GETUTCDATE() AS DATE) OR w.IsActive = 0 THEN 1 ELSE 0 END) AS Expired,
                SUM(CASE WHEN w.IsActive = 1 AND w.EndDate >= CAST(GETUTCDATE() AS DATE)
                          AND w.EndDate <= DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE)) THEN 1 ELSE 0 END) AS Expiring30
            FROM Warranties w
            """;
        return await _db.QueryFirstOrDefaultAsync<WarrantySummaryCounts>(sql) ?? new WarrantySummaryCounts();
    }

    public async Task<IReadOnlyList<WarrantyReportItem>> GetExpiringAsync(int daysAhead = 90) =>
        await _db.QueryAsync<WarrantyReportItem>("""
            SELECT d.AssetTag, d.DeviceName, w.Provider, w.EndDate,
                   DATEDIFF(DAY, CAST(GETUTCDATE() AS DATE), w.EndDate) AS DaysRemaining
            FROM Warranties w INNER JOIN Devices d ON w.DeviceId = d.Id
            WHERE w.IsActive = 1 AND w.EndDate <= DATEADD(DAY, @Days, CAST(GETUTCDATE() AS DATE))
              AND w.EndDate >= CAST(GETUTCDATE() AS DATE)
            ORDER BY w.EndDate ASC
            """, new { Days = daysAhead });

    public async Task<IReadOnlyList<Warranty>> GetByDeviceIdAsync(int deviceId) =>
        await _db.QueryAsync<Warranty>($"{SelectSql} WHERE w.DeviceId = @DeviceId ORDER BY w.EndDate DESC", new { DeviceId = deviceId });

    public async Task<IReadOnlyList<MonthlyTrendPoint>> GetExpirationTrendsAsync(int months = 6) =>
        await _db.QueryAsync<MonthlyTrendPoint>("""
            SELECT FORMAT(DATEFROMPARTS(YEAR(EndDate), MONTH(EndDate), 1), 'yyyy-MM') AS Month,
                   COUNT(*) AS Count
            FROM Warranties
            WHERE IsActive = 1
              AND EndDate >= CAST(GETUTCDATE() AS DATE)
              AND EndDate < DATEADD(MONTH, @Months, DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1))
            GROUP BY YEAR(EndDate), MONTH(EndDate)
            ORDER BY YEAR(EndDate), MONTH(EndDate)
            """, new { Months = months });

    private static string BuildWhereClause(SearchFilter filter, out DynamicParameters parameters)
    {
        var where = "WHERE 1=1";
        parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            where += " AND (d.DeviceName LIKE @Search OR d.AssetTag LIKE @Search OR w.Provider LIKE @Search)";
            parameters.Add("Search", $"%{filter.SearchTerm.Trim()}%");
        }

        switch (filter.Status)
        {
            case WarrantyFilterStatus.Active:
                where += " AND w.IsActive = 1 AND w.EndDate > DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE))";
                break;
            case WarrantyFilterStatus.Expiring30:
                where += """
                     AND w.IsActive = 1 AND w.EndDate >= CAST(GETUTCDATE() AS DATE)
                     AND w.EndDate <= DATEADD(DAY, 30, CAST(GETUTCDATE() AS DATE))
                     """;
                break;
            case WarrantyFilterStatus.Expired:
                where += " AND (w.EndDate < CAST(GETUTCDATE() AS DATE) OR w.IsActive = 0)";
                break;
        }

        if (filter.DeviceId.HasValue)
        {
            where += " AND w.DeviceId = @DeviceId";
            parameters.Add("DeviceId", filter.DeviceId.Value);
        }

        return where;
    }
}
