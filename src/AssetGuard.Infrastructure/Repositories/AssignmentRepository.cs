using Dapper;
using AssetGuard.Core.Common;
using AssetGuard.Core.DTOs;
using AssetGuard.Core.Interfaces;
using AssetGuard.Core.Models;

namespace AssetGuard.Infrastructure.Repositories;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly IDapperExecutor _db;

    private const string SelectSql = """
        SELECT a.*, d.DeviceName, d.AssetTag, e.FullName AS EmployeeName, e.Department
        FROM Assignments a
        INNER JOIN Devices d ON a.DeviceId = d.Id
        INNER JOIN Employees e ON a.EmployeeId = e.Id
        """;

    public AssignmentRepository(IDapperExecutor db) => _db = db;

    public async Task<PagedResult<Assignment>> GetPagedAsync(SearchFilter filter)
    {
        var where = BuildWhereClause(filter, out var parameters);
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

        var (total, items) = await _db.QueryPagedAsync<Assignment>(
            $"SELECT COUNT(*) FROM Assignments a INNER JOIN Devices d ON a.DeviceId = d.Id INNER JOIN Employees e ON a.EmployeeId = e.Id {where}",
            $"""
            {SelectSql} {where}
            ORDER BY a.AssignedDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """,
            parameters);

        return new PagedResult<Assignment> { Items = items, TotalCount = total, Page = filter.Page, PageSize = filter.PageSize };
    }

    public Task<Assignment?> GetByIdAsync(int id) =>
        _db.QueryFirstOrDefaultAsync<Assignment>($"{SelectSql} WHERE a.Id = @Id", new { Id = id });

    public Task<int> CreateAsync(Assignment assignment) =>
        _db.ExecuteScalarAsync<int>("""
            INSERT INTO Assignments (DeviceId, EmployeeId, AssignedDate, ReturnDate, Status, AssignmentNote, ReturnNote)
            OUTPUT INSERTED.Id
            VALUES (@DeviceId, @EmployeeId, @AssignedDate, @ReturnDate, @Status, @AssignmentNote, @ReturnNote)
            """, assignment);

    public Task UpdateAsync(Assignment assignment) =>
        _db.ExecuteAsync("""
            UPDATE Assignments SET DeviceId=@DeviceId, EmployeeId=@EmployeeId, AssignedDate=@AssignedDate,
            ReturnDate=@ReturnDate, Status=@Status, AssignmentNote=@AssignmentNote, ReturnNote=@ReturnNote WHERE Id=@Id
            """, assignment);

    public Task ReturnAsync(int id, DateTime returnDate, string? returnNote) =>
        _db.ExecuteAsync("""
            UPDATE Assignments SET ReturnDate=@ReturnDate, ReturnNote=@ReturnNote, Status='Returned' WHERE Id=@Id
            """, new { Id = id, ReturnDate = returnDate, ReturnNote = returnNote });

    public Task<int> GetActiveCountAsync() =>
        _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Assignments WHERE Status = 'Active'");

    public async Task<IReadOnlyList<Assignment>> GetByDeviceIdAsync(int deviceId) =>
        await _db.QueryAsync<Assignment>($"{SelectSql} WHERE a.DeviceId = @DeviceId ORDER BY a.AssignedDate DESC", new { DeviceId = deviceId });

    public async Task<IReadOnlyList<Assignment>> GetByEmployeeIdAsync(int employeeId) =>
        await _db.QueryAsync<Assignment>($"{SelectSql} WHERE a.EmployeeId = @EmployeeId ORDER BY a.AssignedDate DESC", new { EmployeeId = employeeId });

    public async Task<IReadOnlyList<ChartDataPoint>> GetCountByDepartmentAsync() =>
        await _db.QueryAsync<ChartDataPoint>("""
            SELECT e.Department AS Label, COUNT(*) AS Value
            FROM Assignments a INNER JOIN Employees e ON a.EmployeeId = e.Id
            WHERE a.Status = 'Active' GROUP BY e.Department ORDER BY Value DESC
            """);

    public async Task<IReadOnlyList<MonthlyTrendPoint>> GetMonthlyAssignmentTrendsAsync(int months = 6) =>
        await _db.QueryAsync<MonthlyTrendPoint>("""
            SELECT FORMAT(DATEFROMPARTS(YEAR(AssignedDate), MONTH(AssignedDate), 1), 'yyyy-MM') AS Month,
                   COUNT(*) AS Count
            FROM Assignments
            WHERE AssignedDate >= DATEADD(MONTH, -@Months, DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1))
            GROUP BY YEAR(AssignedDate), MONTH(AssignedDate)
            ORDER BY YEAR(AssignedDate), MONTH(AssignedDate)
            """, new { Months = months - 1 });

    private static string BuildWhereClause(SearchFilter filter, out DynamicParameters parameters)
    {
        var where = "WHERE 1=1";
        parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            where += " AND (d.DeviceName LIKE @Search OR d.AssetTag LIKE @Search OR e.FullName LIKE @Search)";
            parameters.Add("Search", $"%{filter.SearchTerm.Trim()}%");
        }
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            where += " AND a.Status = @Status";
            parameters.Add("Status", filter.Status);
        }
        if (filter.DeviceId.HasValue)
        {
            where += " AND a.DeviceId = @DeviceId";
            parameters.Add("DeviceId", filter.DeviceId.Value);
        }
        if (filter.EmployeeId.HasValue)
        {
            where += " AND a.EmployeeId = @EmployeeId";
            parameters.Add("EmployeeId", filter.EmployeeId.Value);
        }

        return where;
    }
}
