using AssetGuard.Core.Interfaces;
using AssetGuard.Core.Models;

namespace AssetGuard.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDapperExecutor _db;

    public UserRepository(IDapperExecutor db) => _db = db;

    public Task<User?> GetByUserNameAsync(string userName) =>
        _db.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE UserName = @UserName AND IsActive = 1", new { UserName = userName });

    public Task<User?> GetByIdAsync(int id) =>
        _db.QueryFirstOrDefaultAsync<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = id });

    public Task<IReadOnlyList<User>> GetAllAsync() =>
        _db.QueryAsync<User>("SELECT * FROM Users ORDER BY UserName");

    public Task<int> CreateAsync(User user) =>
        _db.ExecuteScalarAsync<int>("""
            INSERT INTO Users (UserName, Email, PasswordHash, FullName, Role, IsActive, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@UserName, @Email, @PasswordHash, @FullName, @Role, @IsActive, GETUTCDATE())
            """, user);

    public Task DeactivateAsync(int id) =>
        _db.ExecuteAsync("UPDATE Users SET IsActive = 0 WHERE Id = @Id", new { Id = id });

    public Task<bool> UserNameExistsAsync(string userName, int? excludeId = null) =>
        _db.ExecuteScalarAsync<bool>("""
            SELECT CASE WHEN EXISTS(
                SELECT 1 FROM Users WHERE UserName = @UserName AND (@ExcludeId IS NULL OR Id <> @ExcludeId)
            ) THEN 1 ELSE 0 END
            """, new { UserName = userName, ExcludeId = excludeId });

    public Task UpdateLastLoginAsync(int userId) =>
        _db.ExecuteAsync("UPDATE Users SET LastLoginAt = GETUTCDATE() WHERE Id = @Id", new { Id = userId });
}
