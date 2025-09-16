using HotelReservations.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using HotelReservations.Infrastructure.QuestPdf;

namespace HotelReservations.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly HotelDbContext _context;

        public ReportesController(HotelDbContext context)
        {
            _context = context;
            // Registrar la licencia comunitaria de QuestPDF.
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // GET: Reportes/Pagos - Muestra el formulario de búsqueda
        public IActionResult Pagos()
        {
            var viewModel = new ReportePagosViewModel
            {
                MetodosDePago = new SelectList(new[] { "Efectivo", "Tarjeta de Crédito", "Tarjeta de Débito", "Transferencia" }),
                Resultados = new List<Pago>() // Inicializa con una lista vacía
            };
            return View(viewModel);
        }

        // POST: Reportes/Pagos - Procesa la búsqueda y muestra los resultados
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pagos(ReportePagosViewModel viewModel)
        {
            // Vuelve a poblar la lista de métodos de pago para la vista
            viewModel.MetodosDePago = new SelectList(new[] { "Efectivo", "Tarjeta de Crédito", "Tarjeta de Débito", "Transferencia" }, viewModel.MetodoPago);
            
            // Obtiene los datos filtrados
            viewModel.Resultados = await GetPagosReportDataAsync(viewModel.FechaInicio, viewModel.FechaFin, viewModel.MetodoPago);
            
            return View(viewModel);
        }

        // GET: Reportes/ExportarPagosPDF
        public async Task<IActionResult> ExportarPagosPDF(DateTime? fechaInicio, DateTime? fechaFin, string? metodoPago)
        {
            var pagos = await GetPagosReportDataAsync(fechaInicio, fechaFin, metodoPago);
            var reporteModel = new ReportePagosViewModel
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                MetodoPago = metodoPago,
                Resultados = pagos
            };

            var document = new ReportePagosDocument(reporteModel);
            byte[] pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Reporte_Pagos_{DateTime.Now:yyyyMMdd}.pdf");
        }

        // GET: Reportes/ExportarPagosExcel
        public async Task<IActionResult> ExportarPagosExcel(DateTime? fechaInicio, DateTime? fechaFin, string? metodoPago)
        {
            var pagos = await GetPagosReportDataAsync(fechaInicio, fechaFin, metodoPago);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Pagos");
                var currentRow = 1;

                // Encabezados
                worksheet.Cell(currentRow, 1).Value = "ID Pago";
                worksheet.Cell(currentRow, 2).Value = "Fecha de Pago";
                worksheet.Cell(currentRow, 3).Value = "Monto";
                worksheet.Cell(currentRow, 4).Value = "Método de Pago";
                worksheet.Cell(currentRow, 5).Value = "Reserva ID";
                worksheet.Cell(currentRow, 6).Value = "Cliente";

                // Estilo para el encabezado
                var headerRange = worksheet.Range("A1:F1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#00796b"); // Color del tema
                headerRange.Style.Font.FontColor = XLColor.White;

                // Datos
                foreach (var pago in pagos)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = pago.PagoId;
                    worksheet.Cell(currentRow, 2).Value = pago.FechaPago;
                    worksheet.Cell(currentRow, 3).Value = pago.Monto;
                    worksheet.Cell(currentRow, 4).Value = pago.MetodoPago;
                    worksheet.Cell(currentRow, 5).Value = pago.ReservaId;
                    worksheet.Cell(currentRow, 6).Value = pago.Reserva?.Cliente != null ? $"{pago.Reserva.Cliente.Nombre} {pago.Reserva.Cliente.Apellido}" : "N/A";
                }
                
                // Fila de totales
                currentRow += 2;
                worksheet.Cell(currentRow, 5).Value = "Total Recaudado:";
                worksheet.Cell(currentRow, 5).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 6).FormulaA1 = $"=SUM(C2:C{currentRow - 2})";
                worksheet.Cell(currentRow, 6).Style.Font.Bold = true;

                // Formato de celdas
                worksheet.Column(2).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
                worksheet.Column(3).Style.NumberFormat.Format = "$ #,##0";
                worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "$ #,##0";
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Reporte_Pagos_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        private async Task<List<Pago>> GetPagosReportDataAsync(DateTime? fechaInicio, DateTime? fechaFin, string? metodoPago)
        {
            var query = _context.Pagos.Include(p => p.Reserva.Cliente).AsQueryable();

            if (fechaInicio.HasValue)
            {
                query = query.Where(p => p.FechaPago.Date >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                query = query.Where(p => p.FechaPago.Date <= fechaFin.Value.Date);
            }

            if (!string.IsNullOrEmpty(metodoPago))
            {
                query = query.Where(p => p.MetodoPago == metodoPago);
            }

            return await query.OrderByDescending(p => p.FechaPago).ToListAsync();
        }
    }
}