using AssetGuard.Core.Models;
using AssetGuard.Core.Enums;
using AssetGuard.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AssetGuard.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(IAuditLogRepository auditLogRepository, IHttpContextAccessor httpContextAccessor)
    {
        _auditLogRepository = auditLogRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string actionType, string entityName, int? entityId, string description)
    {
        var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
        await _auditLogRepository.CreateAsync(new AuditLog
        {
            UserName = userName,
            ActionType = actionType,
            EntityName = entityName,
            EntityId = entityId,
            Description = description
        });
    }
}
