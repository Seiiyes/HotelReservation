using HotelReservations.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservations.Controllers
{
    [Authorize]
    public class HabitacionesController : Controller
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<HabitacionesController> _logger;

        public HabitacionesController(HotelDbContext context, ILogger<HabitacionesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Habitaciones
        public async Task<IActionResult> Index()
        {
            return View(await _context.Habitaciones.ToListAsync());
        }

        // GET: Habitaciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var habitacion = await _context.Habitaciones
                .FirstOrDefaultAsync(m => m.HabitacionId == id);

            if (habitacion == null) return NotFound();

            return View(habitacion);
        }

        // GET: Habitaciones/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Habitaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Tipo,Capacidad,Precio,Disponible")] Habitacion habitacion)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(habitacion);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Habitación creada correctamente con ID {Id}", habitacion.HabitacionId);
                    TempData["Success"] = "Habitación creada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al guardar la Habitación");
                    ModelState.AddModelError(string.Empty,
                        "Error al guardar en la base de datos: " + (ex.InnerException?.Message ?? ex.Message));
                }
            }
            return View(habitacion);
        }

        // GET: Habitaciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var habitacion = await _context.Habitaciones.FindAsync(id);
            if (habitacion == null) return NotFound();

            return View(habitacion);
        }

        // POST: Habitaciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HabitacionId,Tipo,Capacidad,Precio,Disponible")] Habitacion habitacion)
        {
            if (id != habitacion.HabitacionId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(habitacion);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Habitación actualizada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex, "Error de concurrencia al editar la habitación {HabitacionId}", id);
                    if (!_context.Habitaciones.Any(e => e.HabitacionId == habitacion.HabitacionId))
                        return NotFound();
                    else
                        throw;
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar la Habitación {HabitacionId}", id);
                    ModelState.AddModelError(string.Empty, "Error al actualizar en la base de datos.");
                }
            }
            return View(habitacion);
        }

        // GET: Habitaciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var habitacion = await _context.Habitaciones
                .FirstOrDefaultAsync(m => m.HabitacionId == id);

            if (habitacion == null) return NotFound();

            return View(habitacion);
        }

        // POST: Habitaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);
            if (habitacion == null)
            {
                TempData["Error"] = "La habitación no fue encontrada.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Habitaciones.Remove(habitacion);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Habitación eliminada correctamente.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al eliminar la habitación con ID {HabitacionId}.", id);
                TempData["Error"] = "No se pudo eliminar la habitación. Es posible que tenga reservas asociadas.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
