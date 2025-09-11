using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservations.Models;

namespace HotelReservations.Controllers
{
    public class PagosController : Controller
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<PagosController> _logger;

        public PagosController(HotelDbContext context, ILogger<PagosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Pagos/GetReservaDetails/5
        [HttpGet]
        public async Task<IActionResult> GetReservaDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Habitacion)
                .FirstOrDefaultAsync(r => r.ReservaId == id);

            if (reserva == null || reserva.Cliente == null || reserva.Habitacion == null)
            {
                return NotFound();
            }

            // Obtener servicios adicionales de la reserva
            var serviciosAdicionales = await _context.ServiciosAdicionales
                .Where(s => s.ReservaId == id)
                .ToListAsync();

            var totalServicios = serviciosAdicionales.Sum(s => s.Precio);
            var totalReserva = reserva.Habitacion.Precio + totalServicios;
            
            // Calcular el total ya pagado
            var pagosPrevios = await _context.Pagos
                .Where(p => p.ReservaId == id)
                .SumAsync(p => p.Monto);
            
            var montoPendiente = totalReserva - pagosPrevios;

            return Json(new
            {
                clienteNombre = $"{reserva.Cliente.Nombre} {reserva.Cliente.Apellido}",
                habitacionTipo = reserva.Habitacion.Tipo,
                precioHabitacion = reserva.Habitacion.Precio.ToString("C0"),
                serviciosAdicionales = serviciosAdicionales.Select(s => new
                {
                    nombre = s.Nombre,
                    precio = s.Precio.ToString("C0")
                }),
                totalServicios = totalServicios.ToString("C0"),
                totalReserva = totalReserva.ToString("C0"),
                pagosPrevios = pagosPrevios.ToString("C0"),
                montoPendiente = montoPendiente.ToString("C0"),
                montoPendienteValor = montoPendiente // Para usar en cálculos
            });
        }

        // GET: Pagos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Pagos
                .Include(p => p.Reserva)
                .ThenInclude(r => r.Cliente)
                .ToListAsync());
        }

        // GET: Pagos/Create
        public async Task<IActionResult> Create(int? reservaId)
        {
            if (reservaId.HasValue)
            {
                // Verificar si la reserva ya tiene un pago
                var existePago = await _context.Pagos.AnyAsync(p => p.ReservaId == reservaId);
                if (existePago)
                {
                    TempData["Error"] = "Esta reserva ya tiene un pago registrado.";
                    return RedirectToAction(nameof(Index));
                }

                var reserva = await _context.Reservas
                    .Include(r => r.Cliente)
                    .Include(r => r.Habitacion)
                    .FirstOrDefaultAsync(r => r.ReservaId == reservaId);

                if (reserva == null)
                {
                    return NotFound();
                }

                if (reserva.Habitacion == null)
                {
                    TempData["Error"] = "La reserva no tiene una habitación asignada.";
                    return RedirectToAction(nameof(Index));
                }

                ViewData["Reserva"] = reserva;
                // Calcular el monto total y pendiente
                var totalServicios = await _context.ServiciosAdicionales
                    .Where(s => s.ReservaId == reservaId)
                    .SumAsync(s => s.Precio);
                
                var totalReserva = reserva.Habitacion.Precio + totalServicios;
                
                // Calcular pagos previos
                var pagosPrevios = await _context.Pagos
                    .Where(p => p.ReservaId == reservaId)
                    .SumAsync(p => p.Monto);
                
                var montoPendiente = totalReserva - pagosPrevios;

                if (montoPendiente <= 0)
                {
                    TempData["Error"] = "La reserva ya está pagada en su totalidad.";
                    return RedirectToAction(nameof(Index));
                }

                ViewData["MontoPendiente"] = montoPendiente.ToString("C0");
                ViewData["TotalReserva"] = totalReserva.ToString("C0");
                ViewData["PagosPrevios"] = pagosPrevios.ToString("C0");

                var pago = new Pago 
                { 
                    ReservaId = reserva.ReservaId,
                    Reserva = reserva,
                    Monto = montoPendiente, // Sugerimos el monto pendiente
                    MetodoPago = "Efectivo",
                    FechaPago = DateTime.Now
                };
                return View(pago);
            }

            var reservasSinPago = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Habitacion)
                .Where(r => !_context.Pagos.Any(p => p.ReservaId == r.ReservaId)) // Solo reservas sin pago
                .Where(r => r.Cliente != null && r.Habitacion != null) // Asegurarse de que tenga cliente y habitación
                .ToListAsync();

            ViewData["Reservas"] = reservasSinPago.Select(r => new
                {
                    r.ReservaId,
                    Descripcion = $"Reserva #{r.ReservaId} - {r.Cliente!.Nombre} {r.Cliente.Apellido} - {r.Habitacion!.Tipo}"
                });

            return View();
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Monto,MetodoPago,ReservaId")] Pago pago)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validar que la reserva exista y cargar datos necesarios
                    var reserva = await _context.Reservas
                        .Include(r => r.Habitacion)
                        .Include(r => r.Cliente)
                        .FirstOrDefaultAsync(r => r.ReservaId == pago.ReservaId);

                    if (reserva == null || reserva.Habitacion == null)
                    {
                        ModelState.AddModelError("", "No se encontró la reserva o la habitación.");
                        return View(pago);
                    }

                    // Validar que no haya pagos duplicados
                    var existePago = await _context.Pagos.AnyAsync(p => p.ReservaId == pago.ReservaId);
                    if (existePago)
                    {
                        ModelState.AddModelError("", "Ya existe un pago registrado para esta reserva.");
                        return View(pago);
                    }

                    // Calcular montos
                    var totalServicios = await _context.ServiciosAdicionales
                        .Where(s => s.ReservaId == pago.ReservaId)
                        .SumAsync(s => s.Precio);

                    var totalReserva = reserva.Habitacion.Precio + totalServicios;
                    var pagosPrevios = await _context.Pagos
                        .Where(p => p.ReservaId == pago.ReservaId)
                        .SumAsync(p => p.Monto);
                    var montoPendiente = totalReserva - pagosPrevios;

                    // Validaciones de monto
                    if (pago.Monto <= 0)
                    {
                        ModelState.AddModelError("Monto", "El monto debe ser mayor que cero.");
                        return View(pago);
                    }

                    if (pago.Monto > montoPendiente)
                    {
                        ModelState.AddModelError("Monto", 
                            $"El monto ingresado (${pago.Monto:N0}) excede el monto pendiente (${montoPendiente:N0}).");
                        return View(pago);
                    }

                    // Iniciar transacción
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // Preparar y guardar el pago
                        pago.FechaPago = DateTime.Now;
                        pago.Reserva = reserva;
                        _context.Add(pago);
                        await _context.SaveChangesAsync();

                        // Registrar el pago en servicios adicionales pendientes
                        var serviciosAdicionales = await _context.ServiciosAdicionales
                            .Where(s => s.ReservaId == pago.ReservaId && s.EstadoPago == "Pendiente")
                            .ToListAsync();

                        if (serviciosAdicionales.Any())
                        {
                            foreach (var servicio in serviciosAdicionales)
                            {
                                servicio.EstadoPago = "Pagado";
                                _context.Update(servicio);
                            }
                            await _context.SaveChangesAsync();
                        }

                        // Confirmar transacción
                        await transaction.CommitAsync();
                        
                        _logger.LogInformation("Pago {PagoId} registrado correctamente para la reserva {ReservaId}", 
                            pago.PagoId, pago.ReservaId);
                        
                        TempData["Success"] = $"Pago por ${pago.Monto:N0} registrado correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error al procesar el pago para la reserva {ReservaId}", pago.ReservaId);
                        ModelState.AddModelError("", "Ha ocurrido un error al procesar el pago. Por favor, inténtelo nuevamente.");
                        throw;
                    }

                    _logger.LogInformation("Pago creado: {PagoId}", pago.PagoId);
                    TempData["Success"] = "Pago registrado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear pago");
                    ModelState.AddModelError("", "Ha ocurrido un error al registrar el pago.");
                }
            }

            var reservasSinPago = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Habitacion)
                .Where(r => !_context.Pagos.Any(p => p.ReservaId == r.ReservaId))
                .Where(r => r.Cliente != null && r.Habitacion != null)
                .ToListAsync();

            ViewData["Reservas"] = reservasSinPago.Select(r => new
                {
                    r.ReservaId,
                    Descripcion = $"Reserva #{r.ReservaId} - {r.Cliente!.Nombre} {r.Cliente.Apellido} - {r.Habitacion!.Tipo}"
                });

            return View(pago);
        }

        // GET: Pagos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .Include(p => p.Reserva)
                .ThenInclude(r => r.Cliente)
                .Include(p => p.Reserva)
                .ThenInclude(r => r.Habitacion)
                .FirstOrDefaultAsync(m => m.PagoId == id);

            if (pago == null)
            {
                return NotFound();
            }

            // Obtener servicios adicionales relacionados
            var serviciosAdicionales = await _context.ServiciosAdicionales
                .Where(s => s.ReservaId == pago.ReservaId)
                .ToListAsync();

            ViewData["ServiciosAdicionales"] = serviciosAdicionales;

            return View(pago);
        }

        // GET: Pagos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pago = await _context.Pagos
                .Include(p => p.Reserva)
                .ThenInclude(r => r.Cliente)
                .FirstOrDefaultAsync(m => m.PagoId == id);

            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // POST: Pagos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pago = await _context.Pagos
                    .Include(p => p.Reserva)
                    .FirstOrDefaultAsync(p => p.PagoId == id);

                if (pago == null)
                {
                    TempData["Error"] = "No se encontró el pago especificado.";
                    return RedirectToAction(nameof(Index));
                }

                // Actualizar el estado de los servicios adicionales
                var serviciosAdicionales = await _context.ServiciosAdicionales
                    .Where(s => s.ReservaId == pago.ReservaId && s.EstadoPago == "Pagado")
                    .ToListAsync();

                foreach (var servicio in serviciosAdicionales)
                {
                    servicio.EstadoPago = "Pendiente";
                    _context.Update(servicio);
                }

                _context.Pagos.Remove(pago);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                
                _logger.LogInformation("Pago {PagoId} eliminado correctamente", id);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }
                
                TempData["Success"] = "Pago eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar pago {PagoId}", id);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false });
                }
                
                TempData["Error"] = "Error al eliminar el pago.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}