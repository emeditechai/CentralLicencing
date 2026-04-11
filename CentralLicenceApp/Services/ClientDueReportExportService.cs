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
    public class ClientDueReportExportService : IClientDueReportExportService
    {
        private static readonly string[] Headers =
        {
            "#", "Party Name", "GSTIN", "Invoice No.", "Invoice Date", "Due Date",
            "Total Amount (Rs)", "Received (Rs)", "Balance Due (Rs)", "Overdue Days", "Status"
        };

        public byte[] GenerateExcel(IReadOnlyList<ClientDueRow> items, string? fromDateLabel, string? toDateLabel)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Client Due Report");
            var colCount = Headers.Length;

            // Title
            ws.Cell(1, 1).Value = "Client-wise Due Report";
            ws.Range(1, 1, 1, colCount).Merge().Style
                .Font.SetBold().Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#FEF3C7"));

            // Filter row
            ws.Cell(3, 1).Value = "Invoice Date From";
            ws.Cell(3, 2).Value = fromDateLabel ?? "All";
            ws.Cell(3, 3).Value = "Invoice Date To";
            ws.Cell(3, 4).Value = toDateLabel ?? "All";

            // Header row
            const int headerRow = 5;
            for (var i = 0; i < Headers.Length; i++)
                ws.Cell(headerRow, i + 1).Value = Headers[i];

            ws.Range(headerRow, 1, headerRow, colCount).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#B45309"))
                .Font.SetFontColor(XLColor.White);

            var row = headerRow + 1;
            var sr = 0;
            foreach (var item in items)
            {
                sr++;
                ws.Cell(row, 1).Value = sr;
                ws.Cell(row, 2).Value = item.PartyName;
                ws.Cell(row, 3).Value = item.PartyGSTINNo ?? "—";
                ws.Cell(row, 4).Value = item.InvoiceNo;
                ws.Cell(row, 5).Value = item.InvoiceDate.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                ws.Cell(row, 6).Value = item.DueDate?.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) ?? "—";
                ws.Cell(row, 7).Value = item.TotalAmount;
                ws.Cell(row, 7).Style.NumberFormat.SetFormat("#,##0.00");
                ws.Cell(row, 8).Value = item.ReceivedAmount;
                ws.Cell(row, 8).Style.NumberFormat.SetFormat("#,##0.00");
                ws.Cell(row, 9).Value = item.BalanceDue;
                ws.Cell(row, 9).Style.NumberFormat.SetFormat("#,##0.00");
                if (item.OverdueDays > 0)
                    ws.Cell(row, 9).Style.Font.SetFontColor(XLColor.Red);
                ws.Cell(row, 10).Value = item.OverdueDays > 0 ? item.OverdueDays : 0;
                if (item.OverdueDays > 0)
                    ws.Cell(row, 10).Style.Font.SetFontColor(XLColor.Red);
                ws.Cell(row, 11).Value = item.Status;
                row++;
            }

            // Totals row
            ws.Cell(row, 6).Value = "Grand Total";
            ws.Cell(row, 6).Style.Font.SetBold();
            ws.Cell(row, 7).Value = items.Sum(x => x.TotalAmount);
            ws.Cell(row, 7).Style.NumberFormat.SetFormat("#,##0.00");
            ws.Cell(row, 7).Style.Font.SetBold();
            ws.Cell(row, 8).Value = items.Sum(x => x.ReceivedAmount);
            ws.Cell(row, 8).Style.NumberFormat.SetFormat("#,##0.00");
            ws.Cell(row, 8).Style.Font.SetBold();
            ws.Cell(row, 9).Value = items.Sum(x => x.BalanceDue);
            ws.Cell(row, 9).Style.NumberFormat.SetFormat("#,##0.00");
            ws.Cell(row, 9).Style.Font.SetBold().Font.SetFontColor(XLColor.Red);

            ws.Columns().AdjustToContents();
            ws.Column(2).Width = Math.Min(ws.Column(2).Width, 30); // Party Name
            ws.SheetView.FreezeRows(headerRow);
            ws.Range(headerRow, 1, Math.Max(headerRow, row - 1), colCount).CreateTable();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GeneratePdf(IReadOnlyList<ClientDueRow> items, string? fromDateLabel, string? toDateLabel)
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
                        col.Item().Text("Client-wise Due Report").FontSize(18).SemiBold().FontColor(Colors.Orange.Darken2);
                        col.Item().PaddingTop(4).Text(
                            $"Invoice Date From: {fromDateLabel ?? "All"}    Invoice Date To: {toDateLabel ?? "All"}")
                            .FontSize(9).FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(12).Column(col =>
                    {
                        // Stat cards
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Element(b => BuildStatCard(b, "Outstanding Invoices", items.Count.ToString(CultureInfo.InvariantCulture), Colors.Orange.Lighten4, Colors.Orange.Darken2));
                            row.RelativeItem().PaddingLeft(8).Element(b => BuildStatCard(b, "Total Balance Due", $"Rs {items.Sum(x => x.BalanceDue):N2}", Colors.Red.Lighten4, Colors.Red.Darken2));
                            row.RelativeItem().PaddingLeft(8).Element(b => BuildStatCard(b, "Overdue Invoices", items.Count(x => x.OverdueDays > 0).ToString(CultureInfo.InvariantCulture), Colors.DeepOrange.Lighten4, Colors.DeepOrange.Darken2));
                        });

                        col.Item().PaddingTop(14).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(25);   // #
                                cols.RelativeColumn(1.8f); // Party Name
                                cols.RelativeColumn(1.2f); // GSTIN
                                cols.RelativeColumn(1.0f); // Invoice No
                                cols.RelativeColumn(0.9f); // Invoice Date
                                cols.RelativeColumn(0.9f); // Due Date
                                cols.RelativeColumn(0.9f); // Total
                                cols.RelativeColumn(0.9f); // Received
                                cols.RelativeColumn(0.9f); // Balance
                                cols.RelativeColumn(0.7f); // Overdue
                                cols.RelativeColumn(0.7f); // Status
                            });

                            static IContainer HeaderCell(IContainer c) => c
                                .Background(Colors.Orange.Darken2).PaddingVertical(6).PaddingHorizontal(4)
                                .DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold().FontSize(7.5f));

                            static IContainer BodyCell(IContainer c) => c
                                .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(5).PaddingHorizontal(4)
                                .DefaultTextStyle(x => x.FontSize(7.5f));

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).AlignCenter().Text("#");
                                header.Cell().Element(HeaderCell).Text("Party Name");
                                header.Cell().Element(HeaderCell).Text("GSTIN");
                                header.Cell().Element(HeaderCell).Text("Invoice No.");
                                header.Cell().Element(HeaderCell).Text("Invoice Date");
                                header.Cell().Element(HeaderCell).Text("Due Date");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Total (Rs)");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Received (Rs)");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Balance (Rs)");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Overdue");
                                header.Cell().Element(HeaderCell).Text("Status");
                            });

                            var sr = 0;
                            foreach (var item in items)
                            {
                                sr++;
                                table.Cell().Element(BodyCell).AlignCenter().Text(sr.ToString());
                                table.Cell().Element(BodyCell).Text(item.PartyName);
                                table.Cell().Element(BodyCell).Text(item.PartyGSTINNo ?? "—");
                                table.Cell().Element(BodyCell).Text(item.InvoiceNo);
                                table.Cell().Element(BodyCell).Text(item.InvoiceDate.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture));
                                table.Cell().Element(BodyCell).Text(item.DueDate?.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) ?? "—");
                                table.Cell().Element(BodyCell).AlignRight().Text($"{item.TotalAmount:N2}");
                                table.Cell().Element(BodyCell).AlignRight().Text($"{item.ReceivedAmount:N2}");

                                var balanceCell = table.Cell().Element(BodyCell).AlignRight();
                                if (item.OverdueDays > 0)
                                    balanceCell.Text($"{item.BalanceDue:N2}").FontColor(Colors.Red.Darken1);
                                else
                                    balanceCell.Text($"{item.BalanceDue:N2}");

                                var overdueCell = table.Cell().Element(BodyCell).AlignCenter();
                                if (item.OverdueDays > 0)
                                    overdueCell.Text($"{item.OverdueDays}d").FontColor(Colors.Red.Darken1).SemiBold();
                                else
                                    overdueCell.Text("—");

                                table.Cell().Element(BodyCell).Text(item.Status);
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Generated on ").FontSize(7).FontColor(Colors.Grey.Medium);
                        t.Span(DateTime.Now.ToString("dd MMM yyyy hh:mm tt", CultureInfo.InvariantCulture)).FontSize(7).FontColor(Colors.Grey.Darken1);
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static void BuildStatCard(IContainer container, string title, string value, string bgColor, string textColor)
        {
            container.Background(bgColor).Padding(10).Column(col =>
            {
                col.Item().Text(title).FontSize(8).FontColor(Colors.Grey.Darken2);
                col.Item().PaddingTop(4).Text(value).FontSize(14).SemiBold().FontColor(textColor);
            });
        }
    }
}
