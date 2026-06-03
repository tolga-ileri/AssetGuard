using AssetGuard.Core.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AssetGuard.Web.Services;

public static class PdfReportExporter
{
    static PdfReportExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static byte[] BuildReport(string title, string[] headers, IEnumerable<string[]> rows, string? footer = null)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text("AssetGuard").Bold().FontSize(10).FontColor(Colors.Grey.Medium);
                    col.Item().Text(title).Bold().FontSize(16).FontColor("#1e3a5f");
                    col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}  |  Records: {rows.Count()}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingBottom(8).LineHorizontal(1).LineColor("#1e3a5f");
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        for (var i = 0; i < headers.Length; i++)
                            cols.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        foreach (var h in headers)
                        {
                            header.Cell().Background("#1e3a5f").Padding(6)
                                .Text(h).Bold().FontColor(Colors.White).FontSize(8);
                        }
                    });

                    foreach (var row in rows)
                    {
                        foreach (var cell in row)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(5).Text(cell ?? "");
                        }
                    }
                });

                if (!string.IsNullOrEmpty(footer))
                {
                    page.Footer().AlignRight().Text(footer).Bold().FontSize(10);
                }
            });
        }).GeneratePdf();
    }

    public static byte[] ExportInventory(IReadOnlyList<InventoryReportRow> rows, ReportFilter filter) =>
        BuildReport("Inventory Report",
            ["Asset Tag", "Serial", "Device", "Type", "Brand", "Model", "Status", "Location", "Purchase", "Warranty End"],
            rows.Select(r => new[]
            {
                r.AssetTag, r.SerialNumber, r.DeviceName, r.DeviceType, r.Brand, r.Model,
                r.Status, r.Location ?? "", r.PurchaseDate.ToString("yyyy-MM-dd"),
                r.WarrantyEndDate?.ToString("yyyy-MM-dd") ?? ""
            }));

    public static byte[] ExportAssignedDevices(IReadOnlyList<AssignedDeviceReportRow> rows, ReportFilter filter) =>
        BuildReport("Assigned Devices Report",
            ["Asset Tag", "Device", "Type", "Employee", "Department", "Assigned", "Status", "Note"],
            rows.Select(r => new[]
            {
                r.AssetTag, r.DeviceName, r.DeviceType, r.EmployeeName, r.Department,
                r.AssignedDate.ToString("yyyy-MM-dd"), r.Status, r.AssignmentNote ?? ""
            }));

    public static byte[] ExportMaintenanceCost(MaintenanceCostReportResult result, ReportFilter filter) =>
        BuildReport("Maintenance Cost Report",
            ["Date", "Asset Tag", "Device", "Type", "Cost", "Performed By", "Description"],
            result.Rows.Select(r => new[]
            {
                r.MaintenanceDate.ToString("yyyy-MM-dd"), r.AssetTag, r.DeviceName, r.MaintenanceType,
                r.Cost.ToString("C"), r.PerformedBy, r.Description
            }),
            $"Total Cost: {result.TotalCost:C}");

    public static byte[] ExportWarrantyExpiration(IReadOnlyList<WarrantyExpirationReportRow> rows, ReportFilter filter) =>
        BuildReport("Warranty Expiration Report",
            ["Asset Tag", "Device", "Provider", "Start", "End", "Status", "Days Remaining"],
            rows.Select(r => new[]
            {
                r.AssetTag, r.DeviceName, r.Provider,
                r.StartDate.ToString("yyyy-MM-dd"), r.EndDate.ToString("yyyy-MM-dd"),
                r.WarrantyStatus, r.DaysRemaining.ToString()
            }));

    public static byte[] ExportDepartmentDevices(IReadOnlyList<DepartmentDeviceReportRow> rows, ReportFilter filter) =>
        BuildReport("Department Device Report",
            ["Department", "Employee", "Asset Tag", "Device", "Type", "Status", "Assigned"],
            rows.Select(r => new[]
            {
                r.Department, r.EmployeeName, r.AssetTag, r.DeviceName, r.DeviceType,
                r.DeviceStatus, r.AssignedDate.ToString("yyyy-MM-dd")
            }));
}
