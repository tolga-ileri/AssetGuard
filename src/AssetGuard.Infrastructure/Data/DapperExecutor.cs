using Dapper;
using AssetGuard.Core.Interfaces;

namespace AssetGuard.Infrastructure.Data;

/// <summary>
/// Centralizes Dapper query execution for testability and consistent connection handling.
/// </summary>
public class DapperExecutor : IDapperExecutor
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperExecutor(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<T>(sql, param);
        return rows.AsList();
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, param);
    }

    public async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.ExecuteScalarAsync<T>(sql, param);
        return result ?? throw new InvalidOperationException("ExecuteScalar returned null.");
    }

    public async Task<(int TotalCount, IReadOnlyList<T> Items)> QueryPagedAsync<T>(
        string countSql, string dataSql, object? param = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var total = await connection.ExecuteScalarAsync<int>(countSql, param);
        var items = (await connection.QueryAsync<T>(dataSql, param)).AsList();
        return (total, items);
    }
}
