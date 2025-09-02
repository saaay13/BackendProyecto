using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using BackendProyecto.Models;

public static class PdfCarnetBuilder
{
    public static byte[] BuildCarnet(Carnets c, byte[]? logoBytes = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        // QR (PNG)
        using var qrGen = new QRCodeGenerator();
        using var data = qrGen.CreateQrCode(c.CodigoVerificacion.ToString(), QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(data);
        var qrBytes = qrCode.GetGraphic(8); // tamaño razonable para CR80

        var colorPrimario = Colors.Blue.Medium;
        var colorTexto = Colors.Grey.Darken3;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                const float mm = 72f / 25.4f;
                page.Size(85.6f * mm, 54f * mm);

                page.Margin(8);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9.5f).FontColor(colorTexto));

                page.Content()
                    .Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8)
                    .Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(60).Height(24).AlignLeft().AlignMiddle().Element(e =>
                            {
                                if (logoBytes is not null)
                                    e.Image(logoBytes, ImageScaling.FitArea);
                                else
                                    e.Text("ONG").SemiBold().FontSize(12).FontColor(colorPrimario);
                            });

                            row.RelativeItem().AlignRight().AlignMiddle().Text("CARNET DE VOLUNTARIO")
                               .SemiBold().FontSize(11).FontColor(colorPrimario);
                        });

                        col.Item().PaddingVertical(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Row(row =>
                        {
                            // Datos (izquierda)
                            row.RelativeItem().Column(left =>
                            {
                                left.Spacing(1.2f);

                                left.Item().Text(t =>
                                {
                                    t.Span("Voluntario: ").SemiBold().FontColor(colorPrimario);
                                    t.Span($"{c.Usuario?.Nombre} {c.Usuario?.Apellido}");
                                });

                                left.Item().Text(t =>
                                {
                                    t.Span("ONG: ").SemiBold().FontColor(colorPrimario);
                                    t.Span($"{c.Ong?.NombreOng}");
                                });

                                if (!string.IsNullOrWhiteSpace(c.Beneficios))
                                {
                                    left.Item().Text(t =>
                                    {
                                        t.Span("Beneficios: ").SemiBold().FontColor(colorPrimario);
                                        t.Span(c.Beneficios);
                                    });
                                }

                                left.Item().Text(t =>
                                {
                                    t.Span("Estado: ").SemiBold().FontColor(colorPrimario);
                                    t.Span(c.EstadoInscripcion.ToString());
                                });

                                left.Item().Row(r2 =>
                                {
                                    r2.RelativeItem().Text(t =>
                                    {
                                        t.Span("Emisión: ").SemiBold().FontColor(colorPrimario);
                                        t.Span($"{c.FechaEmision:yyyy-MM-dd}");
                                    });
                                    r2.RelativeItem().Text(t =>
                                    {
                                        t.Span("Vence: ").SemiBold().FontColor(colorPrimario);
                                        t.Span($"{c.FechaVencimiento:yyyy-MM-dd}");
                                    });
                                });

                               
                            });

                            // QR (derecha)
                            row.ConstantItem(80).AlignRight().AlignMiddle().Column(right =>
                            {
                                right.Spacing(3);
                                right.Item().Height(60).Width(60).Image(qrBytes, ImageScaling.FitArea);
                                right.Item().Text("Escanea para verificar").FontSize(8).AlignCenter();
                            });
                        });

                        // Footer dentro del mismo Content
                        col.Item().AlignCenter()
                            .Text("Emitido por Sistema de Voluntariado")
                            .FontSize(8.5f).FontColor(Colors.Grey.Medium);
                    });
            });
        }).GeneratePdf();
    }
}
