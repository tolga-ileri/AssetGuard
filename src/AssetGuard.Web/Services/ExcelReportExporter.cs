using AssetGuard.Core.DTOs;
using AssetGuard.Core.Models;
using ClosedXML.Excel;

namespace AssetGuard.Web.Services;

public static class ExcelReportExporter
{
    private static void StyleHeader(IXLRow row)
    {
        row.Style.Font.Bold = true;
        row.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
        row.Style.Font.FontColor = XLColor.White;
        row.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    private static void StyleTitle(IXLCell cell)
    {
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 14;
        cell.Style.Font.FontColor = XLColor.FromHtml("#1e3a5f");
    }

    private static void AutoFit(IXLWorksheet ws, int colCount)
    {
        for (var c = 1; c <= colCount; c++)
            ws.Column(c).AdjustToContents(2, 50);
        ws.SheetView.FreezeRows(3);
    }

    public static byte[] ExportInventory(IReadOnlyList<InventoryReportRow> rows, ReportFilter filter)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Inventory Report");
        ws.Cell(1, 1).Value = "AssetGuard — Inventory Report";
        StyleTitle(ws.Cell(1, 1));
        ws.Cell(2, 1).Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}  |  Records: {rows.Count}";

        var headers = new[] { "Asset Tag", "Serial Number", "Device Name", "Type", "Brand", "Model", "Status", "Location", "Purchase Date", "Warranty End" };
        for (var i = 0; i < headers.Length; i++) ws.Cell(3, i + 1).Value = headers[i];
        StyleHeader(ws.Row(3));

        var r = 4;
        foreach (var row in rows)
        {
            ws.Cell(r, 1).Value = row.AssetTag;
            ws.Cell(r, 2).Value = row.SerialNumber;
            ws.Cell(r, 3).Value = row.DeviceName;
            ws.Cell(r, 4).Value = row.DeviceType;
            ws.Cell(r, 5).Value = row.Brand;
            ws.Cell(r, 6).Value = row.Model;
            ws.Cell(r, 7).Value = row.Status;
            ws.Cell(r, 8).Value = row.Location ?? "";
            ws.Cell(r, 9).Value = row.PurchaseDate;
            ws.Cell(r, 9).Style.DateFormat.Format = "yyyy-MM-dd";
            if (row.WarrantyEndDate.HasValue)
            {
                ws.Cell(r, 10).Value = row.WarrantyEndDate.Value;
                ws.Cell(r, 10).Style.DateFormat.Format = "yyyy-MM-dd";
            }
            r++;
        }
        AutoFit(ws, headers.Length);
        return SaveWorkbook(wb);
    }

    public static byte[] ExportAssignedDevices(IReadOnlyList<AssignedDeviceReportRow> rows, ReportFilter filter)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Assigned Devices");
        ws.Cell(1, 1).Value = "AssetGuard — Assigned Devices Report";
        StyleTitle(ws.Cell(1, 1));
        ws.Cell(2, 1).Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}  |  Records: {rows.Count}";

        var headers = new[] { "Asset Tag", "Device Name", "Type", "Employee", "Department", "Assigned Date", "Status", "Note" };
        for (var i = 0; i < headers.Length; i++) ws.Cell(3, i + 1).Value = headers[i];
        StyleHeader(ws.Row(3));

        var r = 4;
        foreach (var row in rows)
        {
            ws.Cell(r, 1).Value = row.AssetTag;
            ws.Cell(r, 2).Value = row.DeviceName;
            ws.Cell(r, 3).Value = row.DeviceType;
            ws.Cell(r, 4).Value = row.EmployeeName;
            ws.Cell(r, 5).Value = row.Department;
            ws.Cell(r, 6).Value = row.AssignedDate;
            ws.Cell(r, 6).Style.DateFormat.Format = "yyyy-MM-dd";
            ws.Cell(r, 7).Value = row.Status;
            ws.Cell(r, 8).Value = row.AssignmentNote ?? "";
            r++;
        }
        AutoFit(ws, headers.Length);
        return SaveWorkbook(wb);
    }

    public static byte[] ExportMaintenanceCost(MaintenanceCostReportResult result, ReportFilter filter)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Maintenance Cost");
        ws.Cell(1, 1).Value = "AssetGuard — Maintenance Cost Report";
        StyleTitle(ws.Cell(1, 1));
        ws.Cell(2, 1).Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}  |  Records: {result.RecordCount}  |  Total Cost: {result.TotalCost:C2}";
        ws.Cell(2, 1).Style.Font.Bold = true;

        var headers = new[] { "Date", "Asset Tag", "Device", "Type", "Maintenance Type", "Description", "Cost", "Performed By" };
        for (var i = 0; i < headers.Length; i++) ws.Cell(3, i + 1).Value = headers[i];
        StyleHeader(ws.Row(3));

        var r = 4;
        foreach (var row in result.Rows)
        {
            ws.Cell(r, 1).Value = row.MaintenanceDate;
            ws.Cell(r, 1).Style.DateFormat.Format = "yyyy-MM-dd";
            ws.Cell(r, 2).Value = row.AssetTag;
            ws.Cell(r, 3).Value = row.DeviceName;
            ws.Cell(r, 4).Value = row.DeviceType;
            ws.Cell(r, 5).Value = row.MaintenanceType;
            ws.Cell(r, 6).Value = row.Description;
            ws.Cell(r, 7).Value = row.Cost;
            ws.Cell(r, 7).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(r, 8).Value = row.PerformedBy;
            r++;
        }

        ws.Cell(r + 1, 6).Value = "TOTAL:";
        ws.Cell(r + 1, 6).Style.Font.Bold = true;
        ws.Cell(r + 1, 7).Value = result.TotalCost;
        ws.Cell(r + 1, 7).Style.NumberFormat.Format = "$#,##0.00";
        ws.Cell(r + 1, 7).Style.Font.Bold = true;

        AutoFit(ws, headers.Length);
        return SaveWorkbook(wb);
    }

    public static byte[] ExportWarrantyExpiration(IReadOnlyList<WarrantyExpirationReportRow> rows, ReportFilter filter)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Warranty Expiration");
        ws.Cell(1, 1).Value = "AssetGuard — Warranty Expiration Report";
        StyleTitle(ws.Cell(1, 1));
        ws.Cell(2, 1).Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}  |  Records: {rows.Count}";

        var headers = new[] { "Asset Tag", "Device", "Type", "Provider", "Start Date", "End Date", "Days Remaining", "Status" };
        for (var i = 0; i < headers.Length; i++) ws.Cell(3, i + 1).Value = headers[i];
        StyleHeader(ws.Row(3));

        var r = 4;
        foreach (var row in rows)
        {
            ws.Cell(r, 1).Value = row.AssetTag;
            ws.Cell(r, 2).Value = row.DeviceName;
            ws.Cell(r, 3).Value = row.DeviceType;
            ws.Cell(r, 4).Value = row.Provider;
            ws.Cell(r, 5).Value = row.StartDate;
            ws.Cell(r, 5).Style.DateFormat.Format = "yyyy-MM-dd";
            ws.Cell(r, 6).Value = row.EndDate;
            ws.Cell(r, 6).Style.DateFormat.Format = "yyyy-MM-dd";
            ws.Cell(r, 7).Value = row.DaysRemaining;
            ws.Cell(r, 8).Value = row.WarrantyStatus;
            r++;
        }
        AutoFit(ws, headers.Length);
        return SaveWorkbook(wb);
    }

    public static byte[] ExportDepartmentDevices(IReadOnlyList<DepartmentDeviceReportRow> rows, ReportFilter filter)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Department Devices");
        ws.Cell(1, 1).Value = "AssetGuard — Department Device Report";
        StyleTitle(ws.Cell(1, 1));
        ws.Cell(2, 1).Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}  |  Records: {rows.Count}";

        var headers = new[] { "Department", "Employee", "Asset Tag", "Device", "Type", "Assigned Date", "Device Status" };
        for (var i = 0; i < headers.Length; i++) ws.Cell(3, i + 1).Value = headers[i];
        StyleHeader(ws.Row(3));

        var r = 4;
        foreach (var row in rows)
        {
            ws.Cell(r, 1).Value = row.Department;
            ws.Cell(r, 2).Value = row.EmployeeName;
            ws.Cell(r, 3).Value = row.AssetTag;
            ws.Cell(r, 4).Value = row.DeviceName;
            ws.Cell(r, 5).Value = row.DeviceType;
            ws.Cell(r, 6).Value = row.AssignedDate;
            ws.Cell(r, 6).Style.DateFormat.Format = "yyyy-MM-dd";
            ws.Cell(r, 7).Value = row.DeviceStatus;
            r++;
        }
        AutoFit(ws, headers.Length);
        return SaveWorkbook(wb);
    }

    public static byte[] ExportAuditLogs(IReadOnlyList<AuditLog> logs)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Audit Logs");
        ws.Cell(1, 1).Value = "AssetGuard — Audit Log Export";
        StyleTitle(ws.Cell(1, 1));
        ws.Cell(2, 1).Value = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}  |  Records: {logs.Count}";

        var headers = new[] { "Timestamp", "User", "Action", "Entity", "Entity ID", "Description" };
        for (var i = 0; i < headers.Length; i++) ws.Cell(3, i + 1).Value = headers[i];
        StyleHeader(ws.Row(3));

        var r = 4;
        foreach (var log in logs)
        {
            ws.Cell(r, 1).Value = log.CreatedAt;
            ws.Cell(r, 1).Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
            ws.Cell(r, 2).Value = log.UserName;
            ws.Cell(r, 3).Value = log.ActionType;
            ws.Cell(r, 4).Value = log.EntityName;
            ws.Cell(r, 5).Value = log.EntityId?.ToString() ?? "";
            ws.Cell(r, 6).Value = log.Description;
            r++;
        }
        AutoFit(ws, headers.Length);
        return SaveWorkbook(wb);
    }

    private static byte[] SaveWorkbook(XLWorkbook wb)
    {
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }
}
