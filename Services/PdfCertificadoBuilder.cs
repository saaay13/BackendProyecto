using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
using BackendProyecto.Models;

public static class PdfCertificadoBuilder
{
    public static byte[] BuildCertificado(Certificados cert, byte[]? logoBytes = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        // QR con el código de verificación
        using var qrGen = new QRCodeGenerator();
        using var data = qrGen.CreateQrCode(cert.CodigoVerificacion, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(data);
        var qrBytes = qrCode.GetGraphic(8);

        var prim = Colors.Indigo.Medium;
        var txt = Colors.Grey.Darken3;

        return Document.Create(c =>
        {
            c.Page(page =>
            {
                // A5 horizontal para un certificado más amplio
                const float mm = 72f / 25.4f;
                page.Size(210f * mm, 148f * mm);  // A5 landscape
                page.Margin(24);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontColor(txt));

                page.Content().Column(col =>
                {
                    // Encabezado
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(120).Height(60).AlignLeft().AlignMiddle().Element(e =>
                        {
                            if (logoBytes is not null) e.Image(logoBytes, ImageScaling.FitArea);
                            else e.Text("ONG").SemiBold().FontSize(22).FontColor(prim);
                        });

                        row.RelativeItem().AlignRight().AlignMiddle().Text("CERTIFICADO")
                           .SemiBold().FontSize(24).FontColor(prim);
                    });

                    col.Item().PaddingVertical(8).LineHorizontal(2).LineColor(Colors.Grey.Lighten2);

                    // Cuerpo
                    col.Item().Row(r =>
                    {
                        // Datos principales
                        r.RelativeItem().Column(left =>
                        {
                            left.Spacing(6);

                            var usuarioNombre = $"{cert.Usuario?.Nombre} {cert.Usuario?.Apellido}".Trim();
                            var actividadNombre = cert.Actividad?.NombreActividad ?? "-";
                            var proyectoNombre = cert.Actividad?.Proyecto?.NombreProyecto ?? "-";
                            var ongNombre = cert.Actividad?.Proyecto?.Ong?.NombreOng ?? "-";

                            left.Item().Text(t =>
                            {
                                t.Span("Otorgado a: ").SemiBold().FontColor(prim);
                                t.Span(string.IsNullOrWhiteSpace(usuarioNombre) ? "-" : usuarioNombre)
                                 .FontSize(16).SemiBold();
                            });

                            left.Item().Text(t =>
                            {
                                t.Span("Actividad: ").SemiBold().FontColor(prim);
                                t.Span(actividadNombre);
                            });

                            left.Item().Text(t =>
                            {
                                t.Span("Proyecto: ").SemiBold().FontColor(prim);
                                t.Span(proyectoNombre);
                            });

                            left.Item().Text(t =>
                            {
                                t.Span("ONG: ").SemiBold().FontColor(prim);
                                t.Span(ongNombre);
                            });

                            left.Item().Text(t =>
                            {
                                t.Span("Fecha de emisión: ").SemiBold().FontColor(prim);
                                t.Span($"{cert.FechaEmision:yyyy-MM-dd}");
                            });

                            left.Item().Text(t =>
                            {
                                t.Span("Código de verificación: ").SemiBold().FontColor(prim);
                                t.Span(cert.CodigoVerificacion);
                            });
                        });

                        // QR
                        r.ConstantItem(140).AlignRight().AlignMiddle().Column(right =>
                        {
                            right.Spacing(8);
                            right.Item().Height(120).Width(120).Image(qrBytes, ImageScaling.FitArea);
                            right.Item().Text("Escanee para verificar").FontSize(10).AlignCenter();
                        });
                    });
                });

                page.Footer().AlignCenter()
                    .Text("Emitido por Sistema de Voluntariado")
                    .FontSize(10).FontColor(Colors.Grey.Medium);
            });
        }).GeneratePdf();
    }
}
