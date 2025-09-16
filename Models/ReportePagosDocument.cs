using HotelReservations.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HotelReservations.Infrastructure.QuestPdf
{
    public class ReportePagosDocument : IDocument
    {
        private readonly ReportePagosViewModel _model;

        public ReportePagosDocument(ReportePagosViewModel model)
        {
            _model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(Colors.Teal.Medium);

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Reporte de Pagos").Style(titleStyle);

                    column.Item().Text(text =>
                    {
                        text.Span("Fecha del reporte: ").SemiBold();
                        text.Span($"{DateTime.Now:dd/MM/yyyy}");
                    });

                    if (_model.FechaInicio.HasValue || _model.FechaFin.HasValue)
                    {
                        column.Item().Text(text =>
                        {
                            text.Span("Período: ").SemiBold();
                            text.Span($"{_model.FechaInicio:dd/MM/yyyy} - {_model.FechaFin:dd/MM/yyyy}");
                        });
                    }
                     if (!string.IsNullOrEmpty(_model.MetodoPago))
                    {
                        column.Item().Text(text =>
                        {
                            text.Span("Método de Pago: ").SemiBold();
                            text.Span(_model.MetodoPago);
                        });
                    }
                });
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Spacing(20);
                column.Item().Element(ComposeTable);

                if (_model.Resultados != null && _model.Resultados.Any())
                {
                    column.Item().AlignRight().Text($"Total Recaudado: {_model.TotalRecaudado:C0}").SemiBold().FontSize(14);
                }
                else
                {
                    column.Item().AlignCenter().Text("No se encontraron resultados para los criterios seleccionados.").FontSize(14);
                }
            });
        }

        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                // Definición de columnas
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Fecha
                    columns.RelativeColumn(3); // Cliente
                    columns.RelativeColumn(2); // Reserva #
                    columns.RelativeColumn(3); // Método
                    columns.RelativeColumn(2); // Monto
                });

                // Encabezado de la tabla
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Teal.Lighten2).Padding(5).Text("Fecha").SemiBold();
                    header.Cell().Background(Colors.Teal.Lighten2).Padding(5).Text("Cliente").SemiBold();
                    header.Cell().Background(Colors.Teal.Lighten2).Padding(5).Text("Reserva #").SemiBold();
                    header.Cell().Background(Colors.Teal.Lighten2).Padding(5).Text("Método").SemiBold();
                    header.Cell().Background(Colors.Teal.Lighten2).Padding(5).AlignRight().Text("Monto").SemiBold();
                });

                // Filas de la tabla
                if (_model.Resultados != null)
                {
                    foreach (var pago in _model.Resultados)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(pago.FechaPago.ToString("dd/MM/yyyy"));
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(pago.Reserva?.Cliente != null ? $"{pago.Reserva.Cliente.Nombre} {pago.Reserva.Cliente.Apellido}" : "N/A");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(pago.ReservaId);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(pago.MetodoPago);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"{pago.Monto:C0}");
                    }
                }
            });
        }

        void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("Página ");
                text.CurrentPageNumber();
                text.Span(" de ");
                text.TotalPages();
            });
        }
    }
}