using HotelReservations.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservations.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly HotelDbContext _context;
        private readonly ILogger<ReservasController> _logger;

        public ReservasController(HotelDbContext context, ILogger<ReservasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Actions
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
        public async Task<IActionResult> Create()
        {
            await CargarListasDesplegablesAsync();
            return View();
        }

        // POST: Reserva/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Reserva reserva)
        {
            // Log los datos recibidos
            _logger.LogInformation("Intento de crear reserva: Cliente {ClienteId}, Habitación {HabitacionId}, Fecha Entrada: {FechaEntrada}, Fecha Salida: {FechaSalida}", 
                reserva.ClienteId, reserva.HabitacionId, reserva.FechaEntrada, reserva.FechaSalida);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Modelo inválido al crear reserva. Errores: {@Errors}", 
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    await CargarListasDesplegablesAsync(reserva.ClienteId, reserva.HabitacionId);
                    return View(reserva);
                }

                // Validaciones centralizadas
                if (!await ValidarReservaAsync(reserva))
                {
                    await CargarListasDesplegablesAsync(reserva.ClienteId, reserva.HabitacionId);
                    return View(reserva);
                }
                
                // Crear la reserva
                try
                {
                    await _context.Reservas.AddAsync(reserva);
                    var result = await _context.SaveChangesAsync();
                    
                    if (result > 0)
                    {
                        _logger.LogInformation("Reserva creada exitosamente: {ReservaId}", reserva.ReservaId);
                        TempData["Success"] = "Reserva creada correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("No se pudo guardar la reserva en la base de datos");
                        ModelState.AddModelError("", "Error al guardar la reserva. No se realizaron cambios en la base de datos.");
                    }
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al guardar la reserva en la base de datos");
                    ModelState.AddModelError("", "Error al guardar la reserva. Por favor, inténtelo de nuevo.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no esperado al crear la reserva");
                ModelState.AddModelError("", "Ha ocurrido un error inesperado. Por favor, inténtelo de nuevo.");
            }

            await CargarListasDesplegablesAsync(reserva.ClienteId, reserva.HabitacionId);
            return View(reserva);
        }

        // GET: Reservas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null) return NotFound();
            await CargarListasDesplegablesAsync(reserva.ClienteId, reserva.HabitacionId);
            return View(reserva);
        }

        // POST: Reservas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservaId,ClienteId,HabitacionId,FechaEntrada,FechaSalida")] Reserva reserva)
        {
            if (id != reserva.ReservaId) return NotFound();

            // Validaciones centralizadas
            if (!await ValidarReservaAsync(reserva))
            {
                // No es necesario hacer nada aquí, el error ya está en ModelState
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reserva);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Reserva actualizada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogWarning(ex, "Error de concurrencia al editar la reserva {ReservaId}", id);
                    if (!_context.Reservas.Any(e => e.ReservaId == reserva.ReservaId))
                    {
                        return NotFound();
                    }
                    ModelState.AddModelError(string.Empty, "La reserva fue modificada por otro usuario. Por favor, recargue la página e intente de nuevo.");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al actualizar la reserva {ReservaId}", id);
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar los cambios en la base de datos.");
                }
            }
            
            await CargarListasDesplegablesAsync(reserva.ClienteId, reserva.HabitacionId);
            return View(reserva);
        }
        
        // GET: Reservas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Habitacion)
                .FirstOrDefaultAsync(m => m.ReservaId == id);
            if (reserva == null) return NotFound();
            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                TempData["Error"] = "La reserva no fue encontrada.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Reservas.Remove(reserva);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Reserva eliminada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al eliminar la reserva con ID {ReservaId}", id);
                TempData["Error"] = "No se pudo eliminar la reserva. Es posible que tenga pagos o servicios adicionales asociados.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }
        #endregion

        private async Task CargarListasDesplegablesAsync(int? selectedCliente = null, int? selectedHabitacion = null)
        {
            try
            {
                _logger.LogInformation("Cargando listas desplegables. Cliente seleccionado: {ClienteId}, Habitación seleccionada: {HabitacionId}",
                    selectedCliente, selectedHabitacion);

                // Cargar clientes
                var clientesQuery = await _context.Clientes
                    .OrderBy(c => c.Apellido)
                    .ThenBy(c => c.Nombre)
                    .Select(c => new { c.ClienteId, NombreCompleto = $"{c.Apellido}, {c.Nombre}" })
                    .ToListAsync();

                if (!clientesQuery.Any())
                {
                    _logger.LogWarning("No se encontraron clientes en la base de datos");
                }

                ViewData["ClienteId"] = new SelectList(clientesQuery, "ClienteId", "NombreCompleto", selectedCliente);

                // Cargar habitaciones
                var habitacionesQuery = await _context.Habitaciones
                    .OrderBy(h => h.Tipo)
                    .ThenBy(h => h.Precio)
                    .Select(h => new { h.HabitacionId, Descripcion = $"{h.Tipo} - {h.Precio:C}" })
                    .ToListAsync();

                if (!habitacionesQuery.Any())
                {
                    _logger.LogWarning("No se encontraron habitaciones en la base de datos");
                }

                ViewData["HabitacionId"] = new SelectList(habitacionesQuery, "HabitacionId", "Descripcion", selectedHabitacion);

                _logger.LogInformation("Listas desplegables cargadas exitosamente. Clientes: {ClientesCount}, Habitaciones: {HabitacionesCount}",
                    clientesQuery.Count, habitacionesQuery.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar las listas desplegables");
                throw; // Re-lanzamos la excepción para que sea manejada por el action method
            }
        }

        private async Task<bool> ValidarReservaAsync(Reserva reserva)
        {
            // 1. Validaciones de fechas
            if (reserva.FechaEntrada.Date < DateTime.Today && reserva.ReservaId == 0) // Solo para nuevas reservas
            {
                _logger.LogWarning("Intento de crear reserva con fecha pasada: {FechaEntrada}", reserva.FechaEntrada);
                ModelState.AddModelError("FechaEntrada", "La fecha de entrada no puede ser anterior a hoy.");
            }

            if (reserva.FechaSalida.Date <= reserva.FechaEntrada.Date)
            {
                _logger.LogWarning("Fecha de salida inválida: {FechaSalida} <= {FechaEntrada}", reserva.FechaSalida, reserva.FechaEntrada);
                ModelState.AddModelError("FechaSalida", "La fecha de salida debe ser posterior a la fecha de entrada.");
            }

            // 2. Verificaciones de existencia (solo si el modelo es válido hasta ahora)
            if (ModelState.IsValid)
            {
                if (!await _context.Clientes.AnyAsync(c => c.ClienteId == reserva.ClienteId))
                {
                    _logger.LogWarning("Cliente no encontrado: {ClienteId}", reserva.ClienteId);
                    ModelState.AddModelError("ClienteId", "El cliente seleccionado no existe.");
                }

                if (!await _context.Habitaciones.AnyAsync(h => h.HabitacionId == reserva.HabitacionId))
                {
                    _logger.LogWarning("Habitación no encontrada: {HabitacionId}", reserva.HabitacionId);
                    ModelState.AddModelError("HabitacionId", "La habitación seleccionada no existe.");
                }
            }

            // 3. Verificar disponibilidad (solo si el modelo sigue siendo válido)
            if (ModelState.IsValid)
            {
                var reservaExistente = await _context.Reservas
                    .AnyAsync(r =>
                        r.ReservaId != reserva.ReservaId && // Excluir la reserva actual en caso de edición
                        r.HabitacionId == reserva.HabitacionId &&
                        r.FechaEntrada.Date < reserva.FechaSalida.Date &&
                        r.FechaSalida.Date > reserva.FechaEntrada.Date);

                if (reservaExistente)
                {
                    _logger.LogWarning("Conflicto de reserva para habitación {HabitacionId}", reserva.HabitacionId);
                    ModelState.AddModelError("", "La habitación ya está reservada para las fechas seleccionadas.");
                }
            }

            return ModelState.IsValid;
        }
    }
}