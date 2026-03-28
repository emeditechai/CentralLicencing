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
    public class ClientDetailsReportExportService : IClientDetailsReportExportService
    {
        public byte[] GenerateExcel(IReadOnlyList<ClientDetailsReportRow> items, string? productType, string? fromDateLabel, string? toDateLabel)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Client Details");

            worksheet.Cell(1, 1).Value = "Client Details Report";
            worksheet.Range(1, 1, 1, 8).Merge().Style
                .Font.SetBold()
                .Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#DBEAFE"));

            worksheet.Cell(3, 1).Value = "Product Type";
            worksheet.Cell(3, 2).Value = string.IsNullOrWhiteSpace(productType) ? "All" : productType;
            worksheet.Cell(3, 3).Value = "Start Date From";
            worksheet.Cell(3, 4).Value = fromDateLabel ?? "All";
            worksheet.Cell(3, 5).Value = "Start Date To";
            worksheet.Cell(3, 6).Value = toDateLabel ?? "All";

            var headerRow = 5;
            var headers = new[] { "Client Code", "Client Name", "Product Type", "Contact Number", "Email", "Client Person", "Address", "Products Purchased", "Reference/Internal", "Status", "Start Date" };

            for (var index = 0; index < headers.Length; index++)
            {
                worksheet.Cell(headerRow, index + 1).Value = headers[index];
            }

            worksheet.Range(headerRow, 1, headerRow, headers.Length).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#1D4ED8"))
                .Font.SetFontColor(XLColor.White);

            var row = headerRow + 1;
            foreach (var item in items)
            {
                worksheet.Cell(row, 1).Value = item.ClientCode;
                worksheet.Cell(row, 2).Value = item.ClientName;
                worksheet.Cell(row, 3).Value = item.ProductType;
                worksheet.Cell(row, 4).Value = item.ContactNumber ?? string.Empty;
                worksheet.Cell(row, 5).Value = item.EmailID ?? string.Empty;
                worksheet.Cell(row, 6).Value = item.ClientPersonName ?? string.Empty;
                worksheet.Cell(row, 7).Value = item.Address ?? string.Empty;
                worksheet.Cell(row, 8).Value = item.PurchasedProductSummary;
                worksheet.Cell(row, 9).Value = item.IsInternalUse ? "Internal Use" : item.ReferenceClientCode ?? string.Empty;
                worksheet.Cell(row, 10).Value = item.IsActive ? "Active" : "Inactive";
                worksheet.Cell(row, 11).Value = item.LicenseStartDate;
                worksheet.Cell(row, 11).Style.DateFormat.SetFormat("dd-mmm-yyyy");
                row++;
            }

            worksheet.Columns().AdjustToContents();
            worksheet.Column(7).Width = 28;
            worksheet.Column(8).Width = 42;
            worksheet.SheetView.FreezeRows(headerRow);
            worksheet.Range(headerRow, 1, Math.Max(headerRow, row - 1), headers.Length).CreateTable();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GeneratePdf(IReadOnlyList<ClientDetailsReportRow> items, string? productType, string? fromDateLabel, string? toDateLabel)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(24);
                    page.Size(PageSizes.A4.Landscape());
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(column =>
                    {
                        column.Item().Text("Client Details Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                        column.Item().PaddingTop(4).Text($"Product Type: {FormatFilter(productType)}    Start Date From: {fromDateLabel ?? "All"}    Start Date To: {toDateLabel ?? "All"}")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(12).Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Element(block => BuildStatCard(block, "Total Clients", items.Count.ToString(CultureInfo.InvariantCulture), Colors.Blue.Lighten4, Colors.Blue.Darken2));
                            row.RelativeItem().PaddingLeft(8).Element(block => BuildStatCard(block, "Internal Use", items.Count(x => x.IsInternalUse).ToString(CultureInfo.InvariantCulture), Colors.LightBlue.Lighten4, Colors.LightBlue.Darken3));
                            row.RelativeItem().PaddingLeft(8).Element(block => BuildStatCard(block, "Referenced Clients", items.Count(x => !string.IsNullOrWhiteSpace(x.ReferenceClientCode)).ToString(CultureInfo.InvariantCulture), Colors.DeepPurple.Lighten4, Colors.DeepPurple.Darken2));
                        });

                        column.Item().PaddingTop(14).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.6f);
                                columns.RelativeColumn(1.1f);
                                columns.RelativeColumn(1.6f);
                                columns.RelativeColumn(1.6f);
                                columns.RelativeColumn(2.4f);
                                columns.RelativeColumn(0.9f);
                                columns.RelativeColumn(1.0f);
                            });

                            static IContainer HeaderCell(IContainer container) => container
                                .Background(Colors.Blue.Darken2)
                                .PaddingVertical(6)
                                .PaddingHorizontal(6)
                                .DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold().FontSize(9));

                            static IContainer BodyCell(IContainer container) => container
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(6)
                                .PaddingHorizontal(6)
                                .DefaultTextStyle(x => x.FontSize(8.5f));

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).Text("Client Code");
                                header.Cell().Element(HeaderCell).Text("Client Name");
                                header.Cell().Element(HeaderCell).Text("Product Type");
                                header.Cell().Element(HeaderCell).Text("Contact");
                                header.Cell().Element(HeaderCell).Text("Client Person");
                                header.Cell().Element(HeaderCell).Text("Products Purchased");
                                header.Cell().Element(HeaderCell).Text("Reference");
                                header.Cell().Element(HeaderCell).Text("Start Date");
                            });

                            foreach (var item in items)
                            {
                                table.Cell().Element(BodyCell).Text(item.ClientCode);
                                table.Cell().Element(BodyCell).Text(item.ClientName);
                                table.Cell().Element(BodyCell).Text(item.ProductType);
                                table.Cell().Element(BodyCell).Text(ComposeContact(item));
                                table.Cell().Element(BodyCell).Text(string.IsNullOrWhiteSpace(item.ClientPersonName) ? "-" : item.ClientPersonName);
                                table.Cell().Element(BodyCell).Text(string.IsNullOrWhiteSpace(item.PurchasedProductSummary) ? "-" : item.PurchasedProductSummary);
                                table.Cell().Element(BodyCell).Text(item.IsInternalUse ? "Internal Use" : string.IsNullOrWhiteSpace(item.ReferenceClientCode) ? "-" : item.ReferenceClientCode);
                                table.Cell().Element(BodyCell).Text(item.LicenseStartDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
                            }

                            if (!items.Any())
                            {
                                table.Cell().ColumnSpan(8).Element(BodyCell).AlignCenter().Text("No report records found");
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
            container
                .Background(backgroundColor)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(12)
                .Column(column =>
                {
                    column.Item().Text(title).FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);
                    column.Item().PaddingTop(4).Text(value).FontSize(18).Bold().FontColor(textColor);
                });
        }

        private static string ComposeContact(ClientDetailsReportRow item)
        {
            var contact = string.IsNullOrWhiteSpace(item.ContactNumber) ? string.Empty : item.ContactNumber.Trim();
            var email = string.IsNullOrWhiteSpace(item.EmailID) ? string.Empty : item.EmailID.Trim();

            if (string.IsNullOrWhiteSpace(contact) && string.IsNullOrWhiteSpace(email))
            {
                return "-";
            }

            if (string.IsNullOrWhiteSpace(contact))
            {
                return email;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return contact;
            }

            return $"{contact}\n{email}";
        }

        private static string FormatFilter(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "All" : value.Trim();
        }
    }
}