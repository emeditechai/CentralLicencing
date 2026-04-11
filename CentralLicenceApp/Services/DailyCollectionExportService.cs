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
    public class DailyCollectionExportService : IDailyCollectionExportService
    {
        private static readonly string[] BaseHeaders =
        {
            "#", "Receipt No.", "Payment Date", "Invoice No.", "Party Name",
            "Payment Mode(s)", "Amount Paid (Rs)", "FY"
        };

        private static readonly string[] AdminHeaders =
        {
            "#", "Receipt No.", "Payment Date", "Invoice No.", "Party Name",
            "Payment Mode(s)", "Amount Paid (Rs)", "Collected By", "FY"
        };

        public byte[] GenerateExcel(IReadOnlyList<DailyCollectionRow> items, bool isAdminView, string? fromDateLabel, string? toDateLabel)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Daily Collection Register");
            var headers = isAdminView ? AdminHeaders : BaseHeaders;
            var colCount = headers.Length;

            // Title
            ws.Cell(1, 1).Value = "Daily Collection Register";
            ws.Range(1, 1, 1, colCount).Merge().Style
                .Font.SetBold().Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#DBEAFE"));

            // Filter row
            ws.Cell(3, 1).Value = "Payment Date From";
            ws.Cell(3, 2).Value = fromDateLabel ?? "All";
            ws.Cell(3, 3).Value = "Payment Date To";
            ws.Cell(3, 4).Value = toDateLabel ?? "All";
            if (isAdminView)
            {
                ws.Cell(3, 5).Value = "View";
                ws.Cell(3, 6).Value = "All Users";
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
            var sr = 0;
            foreach (var item in items)
            {
                sr++;
                var col = 1;
                ws.Cell(row, col++).Value = sr;
                ws.Cell(row, col++).Value = item.ReceiptNo;
                ws.Cell(row, col++).Value = item.PaymentDate.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);
                ws.Cell(row, col++).Value = item.InvoiceNo;
                ws.Cell(row, col++).Value = item.PartyName;
                ws.Cell(row, col++).Value = item.PaymentModes;
                ws.Cell(row, col).Value = item.TotalAmountPaid;
                ws.Cell(row, col).Style.NumberFormat.SetFormat("#,##0.00");
                col++;
                if (isAdminView) ws.Cell(row, col++).Value = item.CollectedBy;
                ws.Cell(row, col++).Value = item.FYCode ?? "—";
                row++;
            }

            // Totals row
            var totalLabelCol = isAdminView ? 7 : 6;
            var totalAmountCol = isAdminView ? 8 : 7;
            ws.Cell(row, totalLabelCol - 1).Value = "Grand Total";
            ws.Cell(row, totalLabelCol - 1).Style.Font.SetBold();
            ws.Cell(row, totalLabelCol).Value = items.Sum(x => x.TotalAmountPaid);
            ws.Cell(row, totalLabelCol).Style.NumberFormat.SetFormat("#,##0.00");
            ws.Cell(row, totalLabelCol).Style.Font.SetBold();

            ws.Columns().AdjustToContents();
            ws.Column(5).Width = Math.Min(ws.Column(5).Width, 35); // Party Name
            ws.Column(6).Width = Math.Min(ws.Column(6).Width, 40); // Payment Modes
            ws.SheetView.FreezeRows(headerRow);
            ws.Range(headerRow, 1, Math.Max(headerRow, row - 1), colCount).CreateTable();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GeneratePdf(IReadOnlyList<DailyCollectionRow> items, bool isAdminView, string? fromDateLabel, string? toDateLabel)
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
                        col.Item().Text("Daily Collection Register").FontSize(18).SemiBold().FontColor(Colors.Blue.Darken2);
                        col.Item().PaddingTop(4).Text(
                            $"Payment Date From: {fromDateLabel ?? "All"}    Payment Date To: {toDateLabel ?? "All"}" +
                            (isAdminView ? "    View: All Users" : "    View: Own Collections"))
                            .FontSize(9).FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(12).Column(col =>
                    {
                        // Stat cards
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Element(b => BuildStatCard(b, "Total Collections", items.Count.ToString(CultureInfo.InvariantCulture), Colors.Blue.Lighten4, Colors.Blue.Darken2));
                            row.RelativeItem().PaddingLeft(8).Element(b => BuildStatCard(b, "Total Amount Collected", $"Rs {items.Sum(x => x.TotalAmountPaid):N2}", Colors.Green.Lighten4, Colors.Green.Darken2));
                            row.RelativeItem().PaddingLeft(8).Element(b => BuildStatCard(b, "Unique Invoices", items.Select(x => x.InvoiceNo).Distinct().Count().ToString(CultureInfo.InvariantCulture), Colors.Orange.Lighten4, Colors.Orange.Darken2));
                        });

                        col.Item().PaddingTop(14).Table(table =>
                        {
                            if (isAdminView)
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(25);   // #
                                    cols.RelativeColumn(1.1f); // Receipt No
                                    cols.RelativeColumn(0.9f); // Payment Date
                                    cols.RelativeColumn(1.1f); // Invoice No
                                    cols.RelativeColumn(1.8f); // Party Name
                                    cols.RelativeColumn(2.0f); // Payment Modes
                                    cols.RelativeColumn(0.9f); // Amount
                                    cols.RelativeColumn(1.1f); // Collected By
                                    cols.RelativeColumn(0.6f); // FY
                                });
                            }
                            else
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(30);   // #
                                    cols.RelativeColumn(1.2f); // Receipt No
                                    cols.RelativeColumn(1.0f); // Payment Date
                                    cols.RelativeColumn(1.2f); // Invoice No
                                    cols.RelativeColumn(2.0f); // Party Name
                                    cols.RelativeColumn(2.2f); // Payment Modes
                                    cols.RelativeColumn(1.0f); // Amount
                                    cols.RelativeColumn(0.7f); // FY
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
                                header.Cell().Element(HeaderCell).AlignCenter().Text("#");
                                header.Cell().Element(HeaderCell).Text("Receipt No.");
                                header.Cell().Element(HeaderCell).Text("Payment Date");
                                header.Cell().Element(HeaderCell).Text("Invoice No.");
                                header.Cell().Element(HeaderCell).Text("Party Name");
                                header.Cell().Element(HeaderCell).Text("Payment Mode(s)");
                                header.Cell().Element(HeaderCell).AlignRight().Text("Amount (Rs)");
                                if (isAdminView) header.Cell().Element(HeaderCell).Text("Collected By");
                                header.Cell().Element(HeaderCell).Text("FY");
                            });

                            var sr = 0;
                            foreach (var item in items)
                            {
                                sr++;
                                table.Cell().Element(BodyCell).AlignCenter().Text(sr.ToString());
                                table.Cell().Element(BodyCell).Text(item.ReceiptNo);
                                table.Cell().Element(BodyCell).Text(item.PaymentDate.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture));
                                table.Cell().Element(BodyCell).Text(item.InvoiceNo);
                                table.Cell().Element(BodyCell).Text(item.PartyName);
                                table.Cell().Element(BodyCell).Text(item.PaymentModes);
                                table.Cell().Element(BodyCell).AlignRight().Text($"{item.TotalAmountPaid:N2}");
                                if (isAdminView) table.Cell().Element(BodyCell).Text(item.CollectedBy);
                                table.Cell().Element(BodyCell).Text(item.FYCode ?? "—");
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
