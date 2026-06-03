using Dapper;
using AssetGuard.Core.Common;
using AssetGuard.Core.Interfaces;
using AssetGuard.Core.Models;

namespace AssetGuard.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IDapperExecutor _db;

    public EmployeeRepository(IDapperExecutor db) => _db = db;

    public async Task<PagedResult<Employee>> GetPagedAsync(SearchFilter filter)
    {
        var where = BuildWhereClause(filter, out var parameters);
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

        var (total, items) = await _db.QueryPagedAsync<Employee>(
            $"SELECT COUNT(*) FROM Employees {where}",
            $"""
            SELECT * FROM Employees {where}
            ORDER BY FullName
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """,
            parameters);

        return new PagedResult<Employee> { Items = items, TotalCount = total, Page = filter.Page, PageSize = filter.PageSize };
    }

    public Task<Employee?> GetByIdAsync(int id) =>
        _db.QueryFirstOrDefaultAsync<Employee>("SELECT * FROM Employees WHERE Id = @Id", new { Id = id });

    public Task<int> CreateAsync(Employee employee) =>
        _db.ExecuteScalarAsync<int>("""
            INSERT INTO Employees (FullName, Department, Position, Email, Phone, IsActive, CreatedAt)
            OUTPUT INSERTED.Id VALUES (@FullName, @Department, @Position, @Email, @Phone, @IsActive, GETUTCDATE())
            """, employee);

    public Task UpdateAsync(Employee employee) =>
        _db.ExecuteAsync("""
            UPDATE Employees SET FullName=@FullName, Department=@Department, Position=@Position,
            Email=@Email, Phone=@Phone, IsActive=@IsActive WHERE Id=@Id
            """, employee);

    public Task DeactivateAsync(int id) =>
        _db.ExecuteAsync("UPDATE Employees SET IsActive = 0 WHERE Id = @Id", new { Id = id });

    public async Task<IReadOnlyList<Employee>> GetActiveAsync() =>
        await _db.QueryAsync<Employee>("SELECT * FROM Employees WHERE IsActive = 1 ORDER BY FullName");

    public Task<int> GetActiveCountAsync() =>
        _db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Employees WHERE IsActive = 1");

    public async Task<IReadOnlyList<string>> GetDepartmentsAsync()
    {
        var rows = await _db.QueryAsync<string>("SELECT DISTINCT Department FROM Employees ORDER BY Department");
        return rows.ToList();
    }

    private static string BuildWhereClause(SearchFilter filter, out DynamicParameters parameters)
    {
        var where = "WHERE 1=1";
        parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            where += " AND (FullName LIKE @Search OR Email LIKE @Search OR Department LIKE @Search OR Position LIKE @Search)";
            parameters.Add("Search", $"%{filter.SearchTerm.Trim()}%");
        }
        if (!string.IsNullOrWhiteSpace(filter.Department))
        {
            where += " AND Department = @Department";
            parameters.Add("Department", filter.Department);
        }
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            where += " AND IsActive = @IsActive";
            parameters.Add("IsActive", filter.Status == "Active");
        }

        return where;
    }
}
