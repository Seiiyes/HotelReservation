using HotelReservations.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservations.Controllers
{
    public class ReservasController : Controller
    {
        private readonly HotelDbContext _context;

        public ReservasController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: Reservas
        public async Task<IActionResult> Index()
        {
            var reservas = _context.Reservas
                                   .Include(r => r.Cliente)
                                   .Include(r => r.Habitacion);
            return View(await reservas.ToListAsync());
        }

        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Habitacion)
                .FirstOrDefaultAsync(m => m.ReservaId == id);

            if (reserva == null) return NotFound();

            return View(reserva);
        }

        // GET: Reservas/Create
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Clientes, "ClienteId", "Nombre");
            ViewData["HabitacionId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Habitaciones, "HabitacionId", "Numero");
            return View();
        }

        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReservaId,ClienteId,HabitacionId,FechaEntrada,FechaSalida")] Reserva reserva)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reserva);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(reserva);
        }

        // GET: Reservas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null) return NotFound();

            ViewData["ClienteId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Clientes, "ClienteId", "Nombre", reserva.ClienteId);
            ViewData["HabitacionId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Habitaciones, "HabitacionId", "Numero", reserva.HabitacionId);

            return View(reserva);
        }

        // POST: Reservas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservaId,ClienteId,HabitacionId,FechaEntrada,FechaSalida")] Reserva reserva)
        {
            if (id != reserva.ReservaId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reserva);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Reservas.Any(e => e.ReservaId == reserva.ReservaId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(reserva);
        }

        // POST: Reservas/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
