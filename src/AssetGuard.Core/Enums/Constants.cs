namespace AssetGuard.Core.Enums;

public static class DeviceStatus
{
    public const string Available = "Available";
    public const string Assigned = "Assigned";
    public const string InMaintenance = "In Maintenance";
    public const string Retired = "Retired";

    public static readonly string[] All = [Available, Assigned, InMaintenance, Retired];
}

public static class AssignmentStatus
{
    public const string Active = "Active";
    public const string Returned = "Returned";

    public static readonly string[] All = [Active, Returned];
}

public static class DeviceTypes
{
    public static readonly string[] All =
    [
        "Laptop", "Desktop", "Monitor", "Phone", "Tablet",
        "Printer", "Server", "Network", "Peripheral", "Other"
    ];
}

public static class MaintenanceTypes
{
    public static readonly string[] All =
    [
        "Preventive", "Corrective", "Upgrade", "Inspection", "Other"
    ];
}

public static class AuditActionTypes
{
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Assign = "Assign";
    public const string Return = "Return";
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string Deactivate = "Deactivate";
}

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string ITStaff = "ITStaff";
    public const string Viewer = "Viewer";

    public static readonly string[] All = [Admin, ITStaff, Viewer];
}
