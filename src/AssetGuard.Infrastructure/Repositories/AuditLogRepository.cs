using Dapper;
using AssetGuard.Core.Common;
using AssetGuard.Core.Interfaces;
using AssetGuard.Core.Models;

namespace AssetGuard.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDapperExecutor _db;

    public AuditLogRepository(IDapperExecutor db) => _db = db;

    public async Task<PagedResult<AuditLog>> GetPagedAsync(SearchFilter filter)
    {
        var where = BuildWhereClause(filter, out var parameters);
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

        var (total, items) = await _db.QueryPagedAsync<AuditLog>(
            $"SELECT COUNT(*) FROM AuditLogs {where}",
            $"""
            SELECT * FROM AuditLogs {where}
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """,
            parameters);

        return new PagedResult<AuditLog> { Items = items, TotalCount = total, Page = filter.Page, PageSize = filter.PageSize };
    }

    public async Task<IReadOnlyList<AuditLog>> GetAllForExportAsync(SearchFilter filter)
    {
        var where = BuildWhereClause(filter, out var parameters);
        return await _db.QueryAsync<AuditLog>($"SELECT * FROM AuditLogs {where} ORDER BY CreatedAt DESC", parameters);
    }

    public Task<long> CreateAsync(AuditLog log) =>
        _db.ExecuteScalarAsync<long>("""
            INSERT INTO AuditLogs (UserName, ActionType, EntityName, EntityId, Description, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@UserName, @ActionType, @EntityName, @EntityId, @Description, GETUTCDATE())
            """, log);

    public Task<IReadOnlyList<AuditLog>> GetForDeviceAsync(int deviceId, int limit = 50) =>
        _db.QueryAsync<AuditLog>("""
            SELECT TOP (@Limit) * FROM AuditLogs
            WHERE (EntityName = 'Device' AND EntityId = @DeviceId)
               OR (EntityName = 'Assignment' AND EntityId IN (SELECT Id FROM Assignments WHERE DeviceId = @DeviceId))
               OR (EntityName = 'MaintenanceRecord' AND EntityId IN (SELECT Id FROM MaintenanceRecords WHERE DeviceId = @DeviceId))
               OR (EntityName = 'Warranty' AND EntityId IN (SELECT Id FROM Warranties WHERE DeviceId = @DeviceId))
            ORDER BY CreatedAt DESC
            """, new { DeviceId = deviceId, Limit = limit });

    public Task<IReadOnlyList<AuditLog>> GetForEmployeeAsync(int employeeId, int limit = 50) =>
        _db.QueryAsync<AuditLog>("""
            SELECT TOP (@Limit) * FROM AuditLogs
            WHERE (EntityName = 'Employee' AND EntityId = @EmployeeId)
               OR (EntityName = 'Assignment' AND EntityId IN (SELECT Id FROM Assignments WHERE EmployeeId = @EmployeeId))
            ORDER BY CreatedAt DESC
            """, new { EmployeeId = employeeId, Limit = limit });

    private static string BuildWhereClause(SearchFilter filter, out DynamicParameters parameters)
    {
        var where = "WHERE 1=1";
        parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            where += " AND Description LIKE @Search";
            parameters.Add("Search", $"%{filter.SearchTerm.Trim()}%");
        }
        if (!string.IsNullOrWhiteSpace(filter.UserName))
        {
            where += " AND UserName = @UserName";
            parameters.Add("UserName", filter.UserName);
        }
        if (!string.IsNullOrWhiteSpace(filter.ActionType))
        {
            where += " AND ActionType = @ActionType";
            parameters.Add("ActionType", filter.ActionType);
        }
        if (!string.IsNullOrWhiteSpace(filter.EntityName))
        {
            where += " AND EntityName = @EntityName";
            parameters.Add("EntityName", filter.EntityName);
        }
        if (filter.DateFrom.HasValue)
        {
            where += " AND CreatedAt >= @DateFrom";
            parameters.Add("DateFrom", filter.DateFrom.Value.Date);
        }
        if (filter.DateTo.HasValue)
        {
            where += " AND CreatedAt < @DateTo";
            parameters.Add("DateTo", filter.DateTo.Value.Date.AddDays(1));
        }

        return where;
    }
}
