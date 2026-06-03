namespace AssetGuard.Core.Interfaces;

public interface IDbConnectionFactory
{
    System.Data.IDbConnection CreateConnection();
}

public interface IDapperExecutor
{
    Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null);
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null);
    Task<int> ExecuteAsync(string sql, object? param = null);
    Task<T> ExecuteScalarAsync<T>(string sql, object? param = null);
    Task<(int TotalCount, IReadOnlyList<T> Items)> QueryPagedAsync<T>(string countSql, string dataSql, object? param = null);
}

public interface IAuditService
{
    Task LogAsync(string actionType, string entityName, int? entityId, string description);
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
