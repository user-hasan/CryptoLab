using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CryptoLab.App.Services;

internal static class PdfExportService
{
    private const string Accent = "#19C8A7";
    private const string DarkNavy = "#102033";
    private const string BodyGray = "#2a3544";
    private const string MutedGray = "#5d6b7e";
    private const string CodeBackground = "#0c1522";
    private const string CodeForeground = "#D4D4D4";

    static PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.UseEnvironmentFonts = true;
    }

    public static byte[] Build(AlgorithmPdfData data)
    {
        var arabic = data.Arabic;
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Segoe UI").FontSize(10.5f).FontColor(BodyGray));

                page.Header().PaddingBottom(10).Column(header =>
                {
                    header.Spacing(6);
                    header.Item().Row(row =>
                    {
                        if (data.LogoBytes is not null)
                        {
                            row.ConstantItem(64).Image(data.LogoBytes).FitWidth();
                            row.ConstantItem(12);
                        }
                        row.RelativeItem().Column(app =>
                        {
                            app.Item().Text(t => Rtl(t.Span(data.AppTitle).FontSize(15).Bold().FontColor(DarkNavy), arabic));
                            app.Item().Text(t => Rtl(t.Span(data.AppSubtitle).FontSize(9.5f).FontColor(MutedGray), arabic));
                        });
                    });
                    header.Item().Container().Height(2).Background(Accent);
                    header.Item().Text(t => Rtl(t.Span(data.AlgorithmName).FontSize(19).Bold().FontColor(DarkNavy), arabic));
                    header.Item().Text(t => Rtl(t.Span(data.MetaLine).FontSize(9.5f).FontColor(MutedGray), arabic));
                });

                page.Content().Column(column =>
                {
                    column.Spacing(9);
                    Section(column, data.OverviewTitle, data.Overview, arabic);
                    Section(column, data.HowItWorksTitle, data.HowItWorks, arabic);
                    Section(column, data.MathConceptTitle, data.MathConcept, arabic);

                    column.Item().Row(row => Heading(row, data.AdvantagesTitle, arabic));
                    column.Item().Column(bullets =>
                    {
                        bullets.Spacing(3);
                        foreach (var item in data.Advantages)
                            bullets.Item().Text(t => Rtl(t.Span("• " + item).FontSize(10.5f).FontColor(BodyGray), arabic));
                    });

                    Section(column, data.SecurityAnalysisTitle, data.SecurityNotice, arabic);

                    column.Item().Row(row => Heading(row, data.PythonTitle, arabic));
                    column.Item().Text(t => Rtl(t.Span(data.PythonIntro).FontSize(10.5f).FontColor(BodyGray), arabic));
                    column.Item().Text(t => Rtl(t.Span(data.PythonVerified).FontSize(9.5f).FontColor(Accent), arabic));
                    column.Item().Text(t => Rtl(t.Span(data.PythonLibrariesTitle + ": " + data.PythonLibraries).FontSize(10.5f).FontColor(BodyGray), arabic));
                    column.Item().Row(row => Heading(row, data.PythonFunctionsTitle, arabic));
                    column.Item().Column(functions =>
                    {
                        functions.Spacing(2.5f);
                        foreach (var item in data.PythonFunctions)
                            functions.Item().Text(t => Rtl(t.Span("• " + item).FontSize(9.5f).FontColor(BodyGray), arabic));
                    });
                    column.Item().Row(row => Heading(row, data.PythonCodeTitle, arabic));
                    column.Item().Border(1).BorderColor("#c8d2de").Background(CodeBackground).Padding(9).Column(lines =>
                    {
                        lines.Spacing(0.5f);
                        foreach (var line in data.PythonCode.Split('\n'))
                            lines.Item().Text(t => t.Span(line.Length == 0 ? " " : line.TrimEnd('\r')).FontFamily("Consolas").FontSize(8).FontColor(CodeForeground).DirectionFromLeftToRight());
                    });
                });

                page.Footer().PaddingTop(8).AlignCenter().Text(t =>
                {
                    t.Span("CryptoLab — ").FontSize(8.5f).FontColor(MutedGray);
                    t.CurrentPageNumber().FontSize(8.5f).FontColor(MutedGray);
                    t.Span(" / ").FontSize(8.5f).FontColor(MutedGray);
                    t.TotalPages().FontSize(8.5f).FontColor(MutedGray);
                });
            });
        }).GeneratePdf();
    }

    private static void Section(ColumnDescriptor column, string title, string body, bool arabic)
    {
        column.Item().Row(row => Heading(row, title, arabic));
        column.Item().Text(t => Rtl(t.Span(body).FontSize(10.5f).FontColor(BodyGray), arabic));
    }

    private static void Heading(RowDescriptor row, string title, bool arabic)
    {
        if (arabic)
        {
            row.RelativeItem().PaddingRight(8).Text(t => Rtl(t.Span(title).FontSize(13.5f).SemiBold().FontColor(DarkNavy), arabic));
            row.ConstantItem(5).Container().Height(15).Background(Accent);
        }
        else
        {
            row.ConstantItem(5).Container().Height(15).Background(Accent);
            row.RelativeItem().PaddingLeft(8).Text(t => Rtl(t.Span(title).FontSize(13.5f).SemiBold().FontColor(DarkNavy), arabic));
        }
    }

    private static TextSpanDescriptor Rtl(TextSpanDescriptor span, bool arabic)
    {
        if (arabic) span.DirectionFromRightToLeft();
        return span;
    }
}