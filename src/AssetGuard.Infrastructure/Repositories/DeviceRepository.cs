using Dapper;
using AssetGuard.Core.Common;
using AssetGuard.Core.DTOs;
using AssetGuard.Core.Interfaces;
using AssetGuard.Core.Models;

namespace AssetGuard.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly IDapperExecutor _db;

    public DeviceRepository(IDapperExecutor db) => _db = db;

    public async Task<PagedResult<Device>> GetPagedAsync(SearchFilter filter)
    {
        var where = BuildWhereClause(filter, out var parameters);
        var offset = (filter.Page - 1) * filter.PageSize;
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", filter.PageSize);

        var (total, items) = await _db.QueryPagedAsync<Device>(
            $"SELECT COUNT(*) FROM Devices {where}",
            $"""
            SELECT * FROM Devices {where}
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """,
            parameters);

        return new PagedResult<Device> { Items = items, TotalCount = total, Page = filter.Page, PageSize = filter.PageSize };
    }

    public Task<Device?> GetByIdAsync(int id) =>
        _db.QueryFirstOrDefaultAsync<Device>("SELECT * FROM Devices WHERE Id = @Id", new { Id = id });

    public Task<int> CreateAsync(Device device) =>
        _db.ExecuteScalarAsync<int>("""
            INSERT INTO Devices (AssetTag, SerialNumber, DeviceName, DeviceType, Brand, Model, PurchaseDate, WarrantyEndDate, Status, Location, Notes, CreatedAt, UpdatedAt)
            OUTPUT INSERTED.Id
            VALUES (@AssetTag, @SerialNumber, @DeviceName, @DeviceType, @Brand, @Model, @PurchaseDate, @WarrantyEndDate, @Status, @Location, @Notes, GETUTCDATE(), GETUTCDATE())
            """, device);

    public Task UpdateAsync(Device device) =>
        _db.ExecuteAsync("""
            UPDATE Devices SET AssetTag=@AssetTag, SerialNumber=@SerialNumber, DeviceName=@DeviceName, DeviceType=@DeviceType,
            Brand=@Brand, Model=@Model, PurchaseDate=@PurchaseDate, WarrantyEndDate=@WarrantyEndDate, Status=@Status,
            Location=@Location, Notes=@Notes, UpdatedAt=GETUTCDATE() WHERE Id=@Id
            """, device);

    public Task DeleteAsync(int id) =>
        _db.ExecuteAsync("DELETE FROM Devices WHERE Id = @Id", new { Id = id });

    public async Task<bool> HasActiveAssignmentAsync(int deviceId)
    {
        var count = await _db.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM Assignments WHERE DeviceId = @DeviceId AND Status = 'Active'", new { DeviceId = deviceId });
        return count > 0;
    }

    public async Task<bool> AssetTagExistsAsync(string assetTag, int? excludeId = null)
    {
        var sql = excludeId.HasValue
            ? "SELECT COUNT(1) FROM Devices WHERE AssetTag = @AssetTag AND Id <> @ExcludeId"
            : "SELECT COUNT(1) FROM Devices WHERE AssetTag = @AssetTag";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { AssetTag = assetTag, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<IReadOnlyList<Device>> GetAvailableAsync() =>
        await _db.QueryAsync<Device>("SELECT * FROM Devices WHERE Status = 'Available' ORDER BY DeviceName");

    public async Task<IReadOnlyList<ChartDataPoint>> GetCountByTypeAsync() =>
        await _db.QueryAsync<ChartDataPoint>("SELECT DeviceType AS Label, COUNT(*) AS Value FROM Devices GROUP BY DeviceType ORDER BY Value DESC");

    public async Task<IReadOnlyList<ChartDataPoint>> GetCountByStatusAsync() =>
        await _db.QueryAsync<ChartDataPoint>("SELECT Status AS Label, COUNT(*) AS Value FROM Devices GROUP BY Status ORDER BY Value DESC");

    private static string BuildWhereClause(SearchFilter filter, out DynamicParameters parameters)
    {
        var where = "WHERE 1=1";
        parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            where += " AND (AssetTag LIKE @Search OR SerialNumber LIKE @Search OR DeviceName LIKE @Search OR Brand LIKE @Search OR Model LIKE @Search)";
            parameters.Add("Search", $"%{filter.SearchTerm.Trim()}%");
        }
        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            where += " AND Status = @Status";
            parameters.Add("Status", filter.Status);
        }
        if (!string.IsNullOrWhiteSpace(filter.DeviceType))
        {
            where += " AND DeviceType = @DeviceType";
            parameters.Add("DeviceType", filter.DeviceType);
        }

        return where;
    }
}
