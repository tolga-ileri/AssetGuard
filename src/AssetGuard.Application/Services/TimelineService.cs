using AssetGuard.Core.DTOs;
using AssetGuard.Core.Enums;
using AssetGuard.Core.Interfaces;
using AssetGuard.Core.Models;

namespace AssetGuard.Application.Services;

public class TimelineService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public TimelineService(IAuditLogRepository auditLogRepository) => _auditLogRepository = auditLogRepository;

    public IReadOnlyList<ActivityTimelineItem> BuildDeviceTimeline(
        Device device,
        IReadOnlyList<Assignment> assignments,
        IReadOnlyList<MaintenanceRecord> maintenance,
        IReadOnlyList<Warranty> warranties,
        IReadOnlyList<AuditLog> auditLogs)
    {
        var items = new List<ActivityTimelineItem>();

        foreach (var a in assignments)
        {
            items.Add(new ActivityTimelineItem
            {
                OccurredAt = a.AssignedDate,
                Category = "Assignment",
                Title = a.Status == AssignmentStatus.Active ? "Assigned to employee" : "Assignment recorded",
                Description = $"{a.EmployeeName} ({a.Department}) — {a.AssignmentNote ?? "No note"}",
                Icon = "bi-person-check",
                ColorClass = a.Status == AssignmentStatus.Active ? "info" : "secondary"
            });
            if (a.ReturnDate.HasValue)
            {
                items.Add(new ActivityTimelineItem
                {
                    OccurredAt = a.ReturnDate.Value,
                    Category = "Assignment",
                    Title = "Device returned",
                    Description = $"Returned from {a.EmployeeName}. {a.ReturnNote ?? ""}".Trim(),
                    Icon = "bi-arrow-return-left",
                    ColorClass = "warning"
                });
            }
        }

        foreach (var m in maintenance)
        {
            items.Add(new ActivityTimelineItem
            {
                OccurredAt = m.MaintenanceDate,
                Category = "Maintenance",
                Title = $"{m.MaintenanceType} maintenance",
                Description = $"{m.Description} — {m.Cost:C} by {m.PerformedBy}",
                Icon = "bi-tools",
                ColorClass = "warning"
            });
        }

        foreach (var w in warranties)
        {
            items.Add(new ActivityTimelineItem
            {
                OccurredAt = w.StartDate,
                Category = "Warranty",
                Title = $"Warranty with {w.Provider}",
                Description = $"Coverage {w.StartDate:MMM d, yyyy} – {w.EndDate:MMM d, yyyy}",
                Icon = "bi-shield-check",
                ColorClass = w.IsActive ? "success" : "secondary"
            });
        }

        foreach (var log in auditLogs)
        {
            items.Add(new ActivityTimelineItem
            {
                OccurredAt = log.CreatedAt,
                Category = "Audit",
                Title = $"{log.ActionType} — {log.EntityName}",
                Description = log.Description,
                Icon = GetAuditIcon(log.ActionType),
                ColorClass = GetAuditColor(log.ActionType),
                Actor = log.UserName
            });
        }

        items.Add(new ActivityTimelineItem
        {
            OccurredAt = device.CreatedAt,
            Category = "System",
            Title = "Device registered",
            Description = $"{device.AssetTag} added to inventory as {device.DeviceType}",
            Icon = "bi-laptop",
            ColorClass = "primary"
        });

        return items.OrderByDescending(i => i.OccurredAt).ToList();
    }

    public IReadOnlyList<ActivityTimelineItem> BuildEmployeeTimeline(
        Employee employee,
        IReadOnlyList<Assignment> assignments,
        IReadOnlyList<AuditLog> auditLogs)
    {
        var items = new List<ActivityTimelineItem>();

        foreach (var a in assignments)
        {
            items.Add(new ActivityTimelineItem
            {
                OccurredAt = a.AssignedDate,
                Category = "Assignment",
                Title = $"Received {a.AssetTag}",
                Description = $"{a.DeviceName} — {a.AssignmentNote ?? "No note"}",
                Icon = "bi-laptop",
                ColorClass = "info"
            });
            if (a.ReturnDate.HasValue)
            {
                items.Add(new ActivityTimelineItem
                {
                    OccurredAt = a.ReturnDate.Value,
                    Category = "Assignment",
                    Title = $"Returned {a.AssetTag}",
                    Description = a.ReturnNote ?? "Device returned to inventory",
                    Icon = "bi-arrow-return-left",
                    ColorClass = "warning"
                });
            }
        }

        foreach (var log in auditLogs)
        {
            items.Add(new ActivityTimelineItem
            {
                OccurredAt = log.CreatedAt,
                Category = "Audit",
                Title = $"{log.ActionType} — {log.EntityName}",
                Description = log.Description,
                Icon = GetAuditIcon(log.ActionType),
                ColorClass = GetAuditColor(log.ActionType),
                Actor = log.UserName
            });
        }

        items.Add(new ActivityTimelineItem
        {
            OccurredAt = employee.CreatedAt,
            Category = "System",
            Title = "Employee onboarded",
            Description = $"{employee.FullName} joined {employee.Department} as {employee.Position}",
            Icon = "bi-person-plus",
            ColorClass = "primary"
        });

        return items.OrderByDescending(i => i.OccurredAt).ToList();
    }

    private static string GetAuditIcon(string actionType) => actionType switch
    {
        AuditActionTypes.Create => "bi-plus-circle",
        AuditActionTypes.Update => "bi-pencil",
        AuditActionTypes.Delete => "bi-trash",
        AuditActionTypes.Assign => "bi-arrow-left-right",
        AuditActionTypes.Return => "bi-arrow-return-left",
        AuditActionTypes.Deactivate => "bi-person-x",
        _ => "bi-journal-text"
    };

    private static string GetAuditColor(string actionType) => actionType switch
    {
        AuditActionTypes.Delete => "danger",
        AuditActionTypes.Deactivate => "warning",
        AuditActionTypes.Assign => "info",
        AuditActionTypes.Return => "warning",
        AuditActionTypes.Create => "success",
        _ => "secondary"
    };
}
