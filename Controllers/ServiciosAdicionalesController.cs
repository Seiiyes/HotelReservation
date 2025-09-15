using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelReservations.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelReservations.Controllers
{
    [Authorize]
    public class ServiciosAdicionalesController : Controller
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<ServiciosAdicionalesController> _logger;

        public ServiciosAdicionalesController(HotelDbContext context, ILogger<ServiciosAdicionalesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ServiciosAdicionales
        public async Task<IActionResult> Index()
        {
            return View(await _context.ServiciosAdicionales.Include(s => s.Reserva).ToListAsync());
        }

        // GET: ServiciosAdicionales/Create
        public async Task<IActionResult> Create(int? reservaId)
        {
            if (reservaId.HasValue)
            {
                var reserva = await _context.Reservas
                    .Include(r => r.Cliente)
                    .Include(r => r.Habitacion)
                    .FirstOrDefaultAsync(r => r.ReservaId == reservaId);

                if (reserva == null)
                {
                    return NotFound();
                }

                ViewData["Reserva"] = reserva;
                return View(new ServicioAdicional { 
                    ReservaId = reserva.ReservaId,
                    Nombre = string.Empty,
                    EstadoPago = "Pendiente",
                    Reserva = reserva
                });
            }

            await CargarReservasAsync();

            return View();
        }

        // POST: ServiciosAdicionales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Precio,Descripcion,ReservaId")] ServicioAdicional servicioAdicional)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    servicioAdicional.EstadoPago = "Pendiente";
                    _context.Add(servicioAdicional);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Servicio adicional creado: {ServicioId}", servicioAdicional.ServicioAdicionalId);
                    TempData["Success"] = "Servicio adicional agregado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear servicio adicional");
                    ModelState.AddModelError("", "Ha ocurrido un error al guardar el servicio adicional.");
                }
            }

            await CargarReservasAsync();

            return View(servicioAdicional);
        }

        // GET: ServiciosAdicionales/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicioAdicional = await _context.ServiciosAdicionales
                .Include("Reserva.Cliente")
                .Include("Reserva.Habitacion")
                .FirstOrDefaultAsync(s => s.ServicioAdicionalId == id);

            if (servicioAdicional == null)
            {
                return NotFound();
            }

            ViewData["Reserva"] = servicioAdicional.Reserva;
            return View(servicioAdicional);
        }

        // POST: ServiciosAdicionales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServicioAdicionalId,Nombre,Precio,Descripcion,ReservaId,FechaSolicitud,EstadoPago")] ServicioAdicional servicioAdicional)
        {
            if (id != servicioAdicional.ServicioAdicionalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(servicioAdicional);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Servicio adicional actualizado: {ServicioId}", servicioAdicional.ServicioAdicionalId);
                    TempData["Success"] = "Servicio adicional actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServicioAdicionalExists(servicioAdicional.ServicioAdicionalId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await CargarReservasAsync();
            return View(servicioAdicional);
        }

        // GET: ServiciosAdicionales/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicioAdicional = await _context.ServiciosAdicionales
                .Include(s => s.Reserva)
                .FirstOrDefaultAsync(m => m.ServicioAdicionalId == id);

            if (servicioAdicional == null)
            {
                return NotFound();
            }

            return View(servicioAdicional);
        }

        // POST: ServiciosAdicionales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var servicioAdicional = await _context.ServiciosAdicionales.FindAsync(id);
                if (servicioAdicional != null)
                {
                    _context.ServiciosAdicionales.Remove(servicioAdicional);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Servicio adicional eliminado: {ServicioId}", id);
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }
                    
                    TempData["Success"] = "Servicio adicional eliminado correctamente.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio adicional {ServicioId}", id);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false });
                }
                
                TempData["Error"] = "Error al eliminar el servicio adicional.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ServicioAdicionalExists(int id)
        {
            return _context.ServiciosAdicionales.Any(e => e.ServicioAdicionalId == id);
        }

        #region Private Helpers
        private async Task CargarReservasAsync()
        {
            ViewData["Reservas"] = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Habitacion)
                .Where(r => r.Cliente != null && r.Habitacion != null)
                .Select(r => new
                {
                    r.ReservaId,
                    Descripcion = $"Reserva #{r.ReservaId} - {r.Cliente!.Nombre} {r.Cliente.Apellido} - {r.Habitacion!.Tipo}"
                })
                .ToListAsync();
        }
        #endregion
    }
}
