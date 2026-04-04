using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CentralLicenceApp.Models.Reports;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CentralLicenceApp.Services
{
    public class ExpenseReportExportService : IExpenseReportExportService
    {
        private static readonly string[] BaseHeaders =
        {
            "Request No.", "Request By User", "Purpose of Travel", "Status",
            "Total Amount (₹)", "Items", "Request Date", "Submitted At",
            "Approved By", "Settled By User", "Settlement Amount (₹)"
        };

        private static readonly string[] AdminHeaders =
        {
            "Request No.", "Request By User", "Employee Code", "Purpose of Travel", "Status",
            "Total Amount (₹)", "Items", "Request Date", "Submitted At",
            "Approved By", "Settled By User", "Settlement Amount (₹)"
        };

        public byte[] GenerateExcel(IReadOnlyList<ExpenseReportRow> items, bool isAdminView, string? fromDateLabel, string? toDateLabel)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Expense Report");
            var headers = isAdminView ? AdminHeaders : BaseHeaders;
            var colCount = headers.Length;

            // Title
            ws.Cell(1, 1).Value = "Expense Report Details";
            ws.Range(1, 1, 1, colCount).Merge().Style
                .Font.SetBold().Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#DBEAFE"));

            // Filter row
            ws.Cell(3, 1).Value = "Request Date From";
            ws.Cell(3, 2).Value = fromDateLabel ?? "All";
            ws.Cell(3, 3).Value = "Request Date To";
            ws.Cell(3, 4).Value = toDateLabel ?? "All";
            if (isAdminView)
            {
                ws.Cell(3, 5).Value = "View";
                ws.Cell(3, 6).Value = "All Employees";
            }

            // Header row
            const int headerRow = 5;
            for (var i = 0; i < headers.Length; i++)
                ws.Cell(headerRow, i + 1).Value = headers[i];

            ws.Range(headerRow, 1, headerRow, colCount).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#1D4ED8"))
                .Font.SetFontColor(XLColor.White);

            var row = headerRow + 1;
            foreach (var item in items)
            {
                var col = 1;
                ws.Cell(row, col++).Value = item.RequestNumber;
                ws.Cell(row, col++).Value = item.RequestedByUser;
                if (isAdminView) ws.Cell(row, col++).Value = item.EmployeeCode ?? string.Empty;
                ws.Cell(row, col++).Value = item.PurposeOfTravel;
                ws.Cell(row, col++).Value = FormatStatus(item.Status);
                ws.Cell(row, col).Value = item.TotalAmount;
                ws.Cell(row, col++).Style.NumberFormat.SetFormat("#,##0.00");
                ws.Cell(row, col++).Value = item.ItemCount;
                ws.Cell(row, col).Value = item.CreatedAt;
                ws.Cell(row, col++).Style.DateFormat.SetFormat("dd-mmm-yyyy");
                ws.Cell(row, col).Value = item.SubmittedAt?.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) ?? "—";
                col++;
                ws.Cell(row, col++).Value = item.ApprovedByUser ?? "—";
                ws.Cell(row, col++).Value = item.SettledByUser ?? "—";
                if (item.SettlementAmount.HasValue)
                {
                    ws.Cell(row, col).Value = item.SettlementAmount.Value;
                    ws.Cell(row, col).Style.NumberFormat.SetFormat("#,##0.00");
                }
                else
                {
                    ws.Cell(row, col).Value = "—";
                }
                row++;
            }

            ws.Columns().AdjustToContents();
            ws.Column(isAdminView ? 4 : 3).Width = Math.Min(ws.Column(isAdminView ? 4 : 3).Width, 35); // Purpose
            ws.SheetView.FreezeRows(headerRow);
            ws.Range(headerRow, 1, Math.Max(headerRow, row - 1), colCount).CreateTable();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GeneratePdf(IReadOnlyList<ExpenseReportRow> items, bool isAdminView, string? fromDateLabel, string? toDateLabel)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(24);
                    page.Size(PageSizes.A4.Landscape());
                    page.DefaultTextStyle(x => x.FontSize(8.5f));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Expense Report Details").FontSize(18).SemiBold().FontColor(Colors.Blue.Darken2);
                        col.Item().PaddingTop(4).Text(
                            $"Request Date From: {fromDateLabel ?? "All"}    Request Date To: {toDateLabel ?? "All"}" +
                            (isAdminView ? "    View: All Employees" : "    View: Own Records"))
                            .FontSize(9).FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(12).Column(col =>
                    {
                        // Stat cards
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Element(b => BuildStatCard(b, "Total Requests", items.Count.ToString(CultureInfo.InvariantCulture), Colors.Blue.Lighten4, Colors.Blue.Darken2));
                            row.RelativeItem().PaddingLeft(8).Element(b => BuildStatCard(b, "Total Amount", $"Rs {items.Sum(x => x.TotalAmount):N2}", Colors.LightBlue.Lighten4, Colors.LightBlue.Darken3));
                            row.RelativeItem().PaddingLeft(8).Element(b => BuildStatCard(b, "Settled", items.Count(x => x.SettledAt.HasValue).ToString(CultureInfo.InvariantCulture), Colors.Green.Lighten4, Colors.Green.Darken2));
                        });

                        col.Item().PaddingTop(14).Table(table =>
                        {
                            if (isAdminView)
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(1.1f);  // Request No
                                    cols.RelativeColumn(1.5f);  // Request By
                                    cols.RelativeColumn(0.9f);  // Emp Code
                                    cols.RelativeColumn(2.0f);  // Purpose
                                    cols.RelativeColumn(1.2f);  // Status
                                    cols.RelativeColumn(1.0f);  // Amount
                                    cols.RelativeColumn(0.5f);  // Items
                                    cols.RelativeColumn(1.0f);  // Request Date
                                    cols.RelativeColumn(1.0f);  // Approved By
                                    cols.RelativeColumn(1.5f);  // Settled By
                                    cols.RelativeColumn(1.0f);  // Settlement Amt
                                });
                            }
                            else
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(1.1f);
                                    cols.RelativeColumn(1.6f);
                                    cols.RelativeColumn(2.2f);
                                    cols.RelativeColumn(1.2f);
                                    cols.RelativeColumn(1.0f);
                                    cols.RelativeColumn(0.5f);
                                    cols.RelativeColumn(1.0f);
                                    cols.RelativeColumn(1.1f);
                                    cols.RelativeColumn(1.5f);
                                    cols.RelativeColumn(1.0f);
                                });
                            }

                            static IContainer HeaderCell(IContainer c) => c
                                .Background(Colors.Blue.Darken2).PaddingVertical(6).PaddingHorizontal(5)
                                .DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold().FontSize(8));

                            static IContainer BodyCell(IContainer c) => c
                                .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(5).PaddingHorizontal(5)
                                .DefaultTextStyle(x => x.FontSize(8));

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Request No.");
                                header.Cell().Element(HeaderCell).Text("Request By");
                                if (isAdminView) header.Cell().Element(HeaderCell).Text("Emp. Code");
                                header.Cell().Element(HeaderCell).Text("Purpose of Travel");
                                header.Cell().Element(HeaderCell).Text("Status");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Amount (Rs)");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Items");
                                header.Cell().Element(HeaderCell).Text("Request Date");
                                header.Cell().Element(HeaderCell).Text("Approved By");
                                header.Cell().Element(HeaderCell).Text("Settled By");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Settled Amt");
                            });

                            foreach (var item in items)
                            {
                                table.Cell().Element(BodyCell).Text(item.RequestNumber);
                                table.Cell().Element(BodyCell).Text(item.RequestedByUser);
                                if (isAdminView) table.Cell().Element(BodyCell).Text(item.EmployeeCode ?? "—");
                                table.Cell().Element(BodyCell).Text(item.PurposeOfTravel);
                                table.Cell().Element(BodyCell).Text(FormatStatus(item.Status));
                                table.Cell().Element(BodyCell).AlignRight().Text($"{item.TotalAmount:N2}");
                                table.Cell().Element(BodyCell).AlignCenter().Text(item.ItemCount.ToString(CultureInfo.InvariantCulture));
                                table.Cell().Element(BodyCell).Text(item.CreatedAt.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
                                table.Cell().Element(BodyCell).Text(item.ApprovedByUser ?? "—");
                                table.Cell().Element(BodyCell).Text(item.SettledByUser ?? "—");
                                table.Cell().Element(BodyCell).AlignRight().Text(item.SettlementAmount.HasValue ? $"{item.SettlementAmount.Value:N2}" : "—");
                            }

                            if (!items.Any())
                            {
                                var span = isAdminView ? 11u : 10u;
                                table.Cell().ColumnSpan(span).Element(BodyCell).AlignCenter().Text("No expense records found");
                            }
                        });
                    });

                    page.Footer().AlignRight().Text(text =>
                    {
                        text.Span("Generated on ");
                        text.Span(DateTime.Now.ToString("dd MMM yyyy hh:mm tt", CultureInfo.InvariantCulture)).SemiBold();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static void BuildStatCard(IContainer container, string title, string value, string backgroundColor, string textColor)
        {
            container.Background(backgroundColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
            {
                col.Item().Text(title).FontSize(8).SemiBold().FontColor(Colors.Grey.Darken2);
                col.Item().PaddingTop(3).Text(value).FontSize(16).Bold().FontColor(textColor);
            });
        }

        private static string FormatStatus(string status) => status switch
        {
            "PendingApproval"        => "Pending Approval",
            "ReimbursementInProcess" => "Reimbursement In Process",
            _                        => status
        };
    }
}
