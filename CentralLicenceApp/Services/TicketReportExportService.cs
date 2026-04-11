using System.Globalization;
using CentralLicenceApp.Models.ViewModels;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CentralLicenceApp.Services
{
    public class TicketReportExportService : ITicketReportExportService
    {
        // ─── Agent Performance ─────────────────────────────────────────

        private static readonly string[] AgentHeaders =
        {
            "#", "Agent Name", "Total Assigned", "Open", "In Progress",
            "Resolved", "Closed", "Avg Response (h)", "Avg Resolution (h)", "Resolution Rate (%)"
        };

        public byte[] GenerateAgentPerformanceExcel(List<AgentPerformanceRow> items, string? fromDateLabel, string? toDateLabel)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Agent Performance");
            var colCount = AgentHeaders.Length;

            // Title
            ws.Cell(1, 1).Value = "Agent Performance Report";
            ws.Range(1, 1, 1, colCount).Merge().Style
                .Font.SetBold().Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#DBEAFE"));

            // Filter row
            ws.Cell(3, 1).Value = "From Date";
            ws.Cell(3, 2).Value = fromDateLabel ?? "All";
            ws.Cell(3, 3).Value = "To Date";
            ws.Cell(3, 4).Value = toDateLabel ?? "All";

            // Summary row
            ws.Cell(3, 6).Value = "Total Agents";
            ws.Cell(3, 7).Value = items.Count;
            ws.Cell(3, 8).Value = "Total Assigned";
            ws.Cell(3, 9).Value = items.Sum(a => a.TotalAssigned);

            // Header row
            const int headerRow = 5;
            for (var i = 0; i < AgentHeaders.Length; i++)
                ws.Cell(headerRow, i + 1).Value = AgentHeaders[i];

            ws.Range(headerRow, 1, headerRow, colCount).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#4F46E5"))
                .Font.SetFontColor(XLColor.White);

            var row = headerRow + 1;
            var idx = 0;
            foreach (var a in items)
            {
                idx++;
                ws.Cell(row, 1).Value = idx;
                ws.Cell(row, 2).Value = a.AgentName;
                ws.Cell(row, 3).Value = a.TotalAssigned;
                ws.Cell(row, 4).Value = a.Open;
                ws.Cell(row, 5).Value = a.InProgress;
                ws.Cell(row, 6).Value = a.Resolved;
                ws.Cell(row, 7).Value = a.Closed;
                ws.Cell(row, 8).Value = Math.Round(a.AvgResponseTimeHours, 1);
                ws.Cell(row, 8).Style.NumberFormat.SetFormat("0.0");
                ws.Cell(row, 9).Value = Math.Round(a.AvgResolutionTimeHours, 1);
                ws.Cell(row, 9).Style.NumberFormat.SetFormat("0.0");
                ws.Cell(row, 10).Value = Math.Round(a.ResolutionRate, 1);
                ws.Cell(row, 10).Style.NumberFormat.SetFormat("0.0");

                // Alternate row shading
                if (idx % 2 == 0)
                    ws.Range(row, 1, row, colCount).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F8FAFC"));

                row++;
            }

            ws.Columns().AdjustToContents();
            ws.SheetView.FreezeRows(headerRow);
            if (items.Any())
                ws.Range(headerRow, 1, row - 1, colCount).CreateTable();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerateAgentPerformancePdf(List<AgentPerformanceRow> items, string? fromDateLabel, string? toDateLabel)
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
                        col.Item().Text("Agent Performance Report").FontSize(18).SemiBold().FontColor(Colors.Indigo.Darken2);
                        col.Item().PaddingTop(4).Text(
                            $"From: {fromDateLabel ?? "All"}    To: {toDateLabel ?? "All"}")
                            .FontSize(9).FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(12).Column(col =>
                    {
                        // Summary cards
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Element(b => BuildStatCard(b, "Total Agents", items.Count.ToString(), Colors.Indigo.Lighten4, Colors.Indigo.Darken2));
                            r.RelativeItem().PaddingLeft(8).Element(b => BuildStatCard(b, "Total Assigned", items.Sum(a => a.TotalAssigned).ToString(), Colors.Blue.Lighten4, Colors.Blue.Darken2));
                            r.RelativeItem().PaddingLeft(8).Element(b => BuildStatCard(b, "Total Resolved", items.Sum(a => a.Resolved + a.Closed).ToString(), Colors.Green.Lighten4, Colors.Green.Darken2));
                            r.RelativeItem().PaddingLeft(8).Element(b => BuildStatCard(b, "Avg Resolution Rate",
                                (items.Any() ? items.Average(a => a.ResolutionRate).ToString("0.#") : "0") + "%",
                                Colors.Orange.Lighten4, Colors.Orange.Darken2));
                        });

                        col.Item().PaddingTop(14).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(25);   // #
                                cols.RelativeColumn(2.0f); // Agent
                                cols.RelativeColumn(1.0f); // Assigned
                                cols.RelativeColumn(0.8f); // Open
                                cols.RelativeColumn(0.8f); // In Progress
                                cols.RelativeColumn(0.8f); // Resolved
                                cols.RelativeColumn(0.8f); // Closed
                                cols.RelativeColumn(1.0f); // Avg Response
                                cols.RelativeColumn(1.0f); // Avg Resolution
                                cols.RelativeColumn(1.0f); // Rate
                            });

                            static IContainer HeaderCell(IContainer c) => c
                                .Background(Colors.Indigo.Darken2).PaddingVertical(6).PaddingHorizontal(5)
                                .DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold().FontSize(8));

                            static IContainer BodyCell(IContainer c) => c
                                .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(5).PaddingHorizontal(5)
                                .DefaultTextStyle(x => x.FontSize(8));

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).AlignCenter().Text("#");
                                header.Cell().Element(HeaderCell).Text("Agent Name");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Assigned");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Open");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("In Progress");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Resolved");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Closed");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Avg Resp (h)");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Avg Resolve (h)");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Rate (%)");
                            });

                            var idx = 0;
                            foreach (var a in items)
                            {
                                idx++;
                                table.Cell().Element(BodyCell).AlignCenter().Text(idx.ToString());
                                table.Cell().Element(BodyCell).Text(a.AgentName);
                                table.Cell().Element(BodyCell).AlignCenter().Text(a.TotalAssigned.ToString());
                                table.Cell().Element(BodyCell).AlignCenter().Text(a.Open.ToString());
                                table.Cell().Element(BodyCell).AlignCenter().Text(a.InProgress.ToString());
                                table.Cell().Element(BodyCell).AlignCenter().Text(a.Resolved.ToString());
                                table.Cell().Element(BodyCell).AlignCenter().Text(a.Closed.ToString());
                                table.Cell().Element(BodyCell).AlignCenter().Text(a.AvgResponseTimeHours.ToString("0.#"));
                                table.Cell().Element(BodyCell).AlignCenter().Text(a.AvgResolutionTimeHours.ToString("0.#"));
                                table.Cell().Element(BodyCell).AlignCenter().Text(a.ResolutionRate.ToString("0.#"));
                            }

                            if (!items.Any())
                                table.Cell().ColumnSpan(10).Element(BodyCell).AlignCenter().Text("No agent performance data found");
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

        // ─── SLA Compliance ────────────────────────────────────────────

        private static readonly string[] SlaHeaders =
        {
            "#", "Ticket #", "Subject", "Priority", "Status", "Assigned To",
            "SLA Resp (h)", "Actual Resp (h)", "Resp SLA",
            "SLA Resolve (h)", "Actual Resolve (h)", "Resolve SLA"
        };

        public byte[] GenerateSlaComplianceExcel(List<SlaComplianceRow> items, string? fromDateLabel, string? toDateLabel)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("SLA Compliance");
            var colCount = SlaHeaders.Length;

            // Title
            ws.Cell(1, 1).Value = "SLA Compliance Report";
            ws.Range(1, 1, 1, colCount).Merge().Style
                .Font.SetBold().Font.SetFontSize(16)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#DBEAFE"));

            // Filter row
            ws.Cell(3, 1).Value = "From Date";
            ws.Cell(3, 2).Value = fromDateLabel ?? "All";
            ws.Cell(3, 3).Value = "To Date";
            ws.Cell(3, 4).Value = toDateLabel ?? "All";

            // Summary row
            var respMet = items.Count(r => r.ResponseSlaStatus == "Met");
            var respBreached = items.Count(r => r.ResponseSlaStatus == "Breached");
            var resMet = items.Count(r => r.ResolutionSlaStatus == "Met");
            var resBreached = items.Count(r => r.ResolutionSlaStatus == "Breached");

            ws.Cell(3, 6).Value = "Total Tickets";
            ws.Cell(3, 7).Value = items.Count;
            ws.Cell(3, 8).Value = "Resp Met";
            ws.Cell(3, 9).Value = respMet;
            ws.Cell(3, 10).Value = "Resp Breached";
            ws.Cell(3, 11).Value = respBreached;

            // Header row
            const int headerRow = 5;
            for (var i = 0; i < SlaHeaders.Length; i++)
                ws.Cell(headerRow, i + 1).Value = SlaHeaders[i];

            ws.Range(headerRow, 1, headerRow, colCount).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromHtml("#4F46E5"))
                .Font.SetFontColor(XLColor.White);

            var row = headerRow + 1;
            var idx = 0;
            foreach (var r in items)
            {
                idx++;
                ws.Cell(row, 1).Value = idx;
                ws.Cell(row, 2).Value = r.TicketNumber;
                ws.Cell(row, 3).Value = r.Subject;
                ws.Cell(row, 4).Value = r.PriorityName;
                ws.Cell(row, 5).Value = r.Status;
                ws.Cell(row, 6).Value = r.AssignedToName ?? "Unassigned";
                ws.Cell(row, 7).Value = r.SlaResponseHours;
                ws.Cell(row, 7).Style.NumberFormat.SetFormat("0.0");
                ws.Cell(row, 8).Value = r.ActualResponseHours.HasValue ? r.ActualResponseHours.Value : 0;
                ws.Cell(row, 8).Style.NumberFormat.SetFormat("0.0");
                if (!r.ActualResponseHours.HasValue) ws.Cell(row, 8).Value = "—";
                ws.Cell(row, 9).Value = r.ResponseSlaStatus;
                ws.Cell(row, 10).Value = r.SlaResolutionHours;
                ws.Cell(row, 10).Style.NumberFormat.SetFormat("0.0");
                ws.Cell(row, 11).Value = r.ActualResolutionHours.HasValue ? r.ActualResolutionHours.Value : 0;
                ws.Cell(row, 11).Style.NumberFormat.SetFormat("0.0");
                if (!r.ActualResolutionHours.HasValue) ws.Cell(row, 11).Value = "—";
                ws.Cell(row, 12).Value = r.ResolutionSlaStatus;

                // Color-code SLA status cells
                ApplySlaStatusColor(ws.Cell(row, 9), r.ResponseSlaStatus);
                ApplySlaStatusColor(ws.Cell(row, 12), r.ResolutionSlaStatus);

                if (idx % 2 == 0)
                    ws.Range(row, 1, row, colCount).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F8FAFC"));

                row++;
            }

            ws.Columns().AdjustToContents();
            ws.Column(3).Width = Math.Min(ws.Column(3).Width, 35); // Subject column
            ws.SheetView.FreezeRows(headerRow);
            if (items.Any())
                ws.Range(headerRow, 1, row - 1, colCount).CreateTable();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerateSlaCompliancePdf(List<SlaComplianceRow> items, string? fromDateLabel, string? toDateLabel)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var respMet = items.Count(r => r.ResponseSlaStatus == "Met");
            var respBreached = items.Count(r => r.ResponseSlaStatus == "Breached");
            var resMet = items.Count(r => r.ResolutionSlaStatus == "Met");
            var resBreached = items.Count(r => r.ResolutionSlaStatus == "Breached");
            var respDenom = respMet + respBreached;
            var resDenom = resMet + resBreached;
            var respPct = respDenom > 0 ? Math.Round(respMet * 100.0 / respDenom, 1) : 0;
            var resPct = resDenom > 0 ? Math.Round(resMet * 100.0 / resDenom, 1) : 0;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(24);
                    page.Size(PageSizes.A4.Landscape());
                    page.DefaultTextStyle(x => x.FontSize(8f));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("SLA Compliance Report").FontSize(18).SemiBold().FontColor(Colors.Indigo.Darken2);
                        col.Item().PaddingTop(4).Text(
                            $"From: {fromDateLabel ?? "All"}    To: {toDateLabel ?? "All"}")
                            .FontSize(9).FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(12).Column(col =>
                    {
                        // Summary cards
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Element(b => BuildStatCard(b, "Total Tickets", items.Count.ToString(), Colors.Blue.Lighten4, Colors.Blue.Darken2));
                            r.RelativeItem().PaddingLeft(6).Element(b => BuildStatCard(b, "Response SLA Met", $"{respMet} ({respPct}%)", Colors.Green.Lighten4, Colors.Green.Darken2));
                            r.RelativeItem().PaddingLeft(6).Element(b => BuildStatCard(b, "Response Breached", respBreached.ToString(), Colors.Red.Lighten4, Colors.Red.Darken2));
                            r.RelativeItem().PaddingLeft(6).Element(b => BuildStatCard(b, "Resolution SLA Met", $"{resMet} ({resPct}%)", Colors.Green.Lighten4, Colors.Green.Darken2));
                            r.RelativeItem().PaddingLeft(6).Element(b => BuildStatCard(b, "Resolution Breached", resBreached.ToString(), Colors.Red.Lighten4, Colors.Red.Darken2));
                        });

                        col.Item().PaddingTop(14).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(22);   // #
                                cols.RelativeColumn(1.0f); // Ticket #
                                cols.RelativeColumn(2.2f); // Subject
                                cols.RelativeColumn(0.8f); // Priority
                                cols.RelativeColumn(0.9f); // Status
                                cols.RelativeColumn(1.2f); // Assigned
                                cols.RelativeColumn(0.7f); // SLA Resp
                                cols.RelativeColumn(0.7f); // Actual Resp
                                cols.RelativeColumn(0.7f); // Resp SLA
                                cols.RelativeColumn(0.7f); // SLA Resolve
                                cols.RelativeColumn(0.7f); // Actual Resolve
                                cols.RelativeColumn(0.7f); // Resolve SLA
                            });

                            static IContainer HeaderCell(IContainer c) => c
                                .Background(Colors.Indigo.Darken2).PaddingVertical(6).PaddingHorizontal(4)
                                .DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold().FontSize(7));

                            static IContainer BodyCell(IContainer c) => c
                                .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .PaddingVertical(4).PaddingHorizontal(4)
                                .DefaultTextStyle(x => x.FontSize(7.5f));

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCell).AlignCenter().Text("#");
                                header.Cell().Element(HeaderCell).Text("Ticket #");
                                header.Cell().Element(HeaderCell).Text("Subject");
                                header.Cell().Element(HeaderCell).Text("Priority");
                                header.Cell().Element(HeaderCell).Text("Status");
                                header.Cell().Element(HeaderCell).Text("Assigned To");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("SLA Resp");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Act. Resp");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Resp SLA");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("SLA Resol.");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Act. Resol.");
                                header.Cell().Element(HeaderCell).AlignCenter().Text("Resol. SLA");
                            });

                            var idx = 0;
                            foreach (var r in items)
                            {
                                idx++;
                                table.Cell().Element(BodyCell).AlignCenter().Text(idx.ToString());
                                table.Cell().Element(BodyCell).Text(r.TicketNumber);
                                table.Cell().Element(BodyCell).Text(r.Subject.Length > 40 ? r.Subject[..40] + "…" : r.Subject);
                                table.Cell().Element(BodyCell).Text(r.PriorityName);
                                table.Cell().Element(BodyCell).Text(r.Status);
                                table.Cell().Element(BodyCell).Text(r.AssignedToName ?? "Unassigned");
                                table.Cell().Element(BodyCell).AlignCenter().Text(r.SlaResponseHours.ToString("0.#"));
                                table.Cell().Element(BodyCell).AlignCenter().Text(r.ActualResponseHours.HasValue ? r.ActualResponseHours.Value.ToString("0.#") : "—");
                                table.Cell().Element(BodyCell).AlignCenter()
                                    .Text(r.ResponseSlaStatus)
                                    .FontColor(SlaStatusColor(r.ResponseSlaStatus));
                                table.Cell().Element(BodyCell).AlignCenter().Text(r.SlaResolutionHours.ToString("0.#"));
                                table.Cell().Element(BodyCell).AlignCenter().Text(r.ActualResolutionHours.HasValue ? r.ActualResolutionHours.Value.ToString("0.#") : "—");
                                table.Cell().Element(BodyCell).AlignCenter()
                                    .Text(r.ResolutionSlaStatus)
                                    .FontColor(SlaStatusColor(r.ResolutionSlaStatus));
                            }

                            if (!items.Any())
                                table.Cell().ColumnSpan(12).Element(BodyCell).AlignCenter().Text("No SLA compliance data found");
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

        // ─── Helpers ───────────────────────────────────────────────────

        private static void BuildStatCard(IContainer container, string title, string value, string backgroundColor, string textColor)
        {
            container.Background(backgroundColor).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(col =>
            {
                col.Item().Text(title).FontSize(8).SemiBold().FontColor(Colors.Grey.Darken2);
                col.Item().PaddingTop(3).Text(value).FontSize(14).Bold().FontColor(textColor);
            });
        }

        private static void ApplySlaStatusColor(IXLCell cell, string status)
        {
            switch (status)
            {
                case "Met":
                    cell.Style.Font.SetFontColor(XLColor.FromHtml("#16A34A"));
                    cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#DCFCE7"));
                    break;
                case "Breached":
                    cell.Style.Font.SetFontColor(XLColor.FromHtml("#EF4444"));
                    cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#FEE2E2"));
                    break;
                case "Pending":
                    cell.Style.Font.SetFontColor(XLColor.FromHtml("#D97706"));
                    cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#FEF3C7"));
                    break;
            }
        }

        private static string SlaStatusColor(string status) => status switch
        {
            "Met" => Colors.Green.Darken2,
            "Breached" => Colors.Red.Darken2,
            "Pending" => Colors.Orange.Darken2,
            _ => Colors.Grey.Darken2
        };
    }
}
