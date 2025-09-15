using HotelReservations.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace HotelReservations.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(HotelDbContext context, ILogger<ClientesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes.ToListAsync();
            return View(clientes);
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.ClienteId == id);

            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            try
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cliente creado correctamente con ID {Id}", cliente.ClienteId);
                TempData["Success"] = "Cliente creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al guardar Cliente");
                ModelState.AddModelError(string.Empty,
                    "Error al guardar en la base de datos: " + (ex.InnerException?.Message ?? ex.Message));
                return View(cliente);
            }
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cliente cliente)
        {
            if (id != cliente.ClienteId) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            try
            {
                _context.Update(cliente);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cliente actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Clientes.Any(e => e.ClienteId == cliente.ClienteId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar Cliente");
                ModelState.AddModelError(string.Empty,
                    "Error al actualizar en la base de datos: " + (ex.InnerException?.Message ?? ex.Message));
                return View(cliente);
            }
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.ClienteId == id);

            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                TempData["Error"] = "El cliente no fue encontrado.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cliente eliminado correctamente.";
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al eliminar el cliente con ID {ClienteId}", id);
                TempData["Error"] = "No se pudo eliminar el cliente. Es posible que tenga reservas asociadas.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
