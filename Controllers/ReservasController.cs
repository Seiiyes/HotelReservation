using HotelReservations.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservations.Controllers
{
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

                // Validaciones de fechas
                var today = DateTime.Today;
                if (reserva.FechaEntrada.Date < today)
                {
                    _logger.LogWarning("Intento de crear reserva con fecha pasada: {FechaEntrada}", reserva.FechaEntrada);
                    ModelState.AddModelError("FechaEntrada", "La fecha de entrada no puede ser anterior a hoy.");
                    await CargarListasDesplegablesAsync(reserva.ClienteId, reserva.HabitacionId);
                    return View(reserva);
                }

                if (reserva.FechaSalida.Date <= reserva.FechaEntrada.Date)
                {
                    _logger.LogWarning("Fecha de salida inválida: {FechaSalida} <= {FechaEntrada}", 
                        reserva.FechaSalida, reserva.FechaEntrada);
                    ModelState.AddModelError("FechaSalida", "La fecha de salida debe ser posterior a la fecha de entrada.");
                    await CargarListasDesplegablesAsync(reserva.ClienteId, reserva.HabitacionId);
                    return View(reserva);
                }

                // Verificaciones de existencia
                var cliente = await _context.Clientes.FindAsync(reserva.ClienteId);
                if (cliente == null)
                {
                    _logger.LogWarning("Cliente no encontrado: {ClienteId}", reserva.ClienteId);
                    ModelState.AddModelError("ClienteId", "El cliente seleccionado no existe.");
                    await CargarListasDesplegablesAsync(reserva.ClienteId, reserva.HabitacionId);
                    return View(reserva);
                }

                var habitacion = await _context.Habitaciones.FindAsync(reserva.HabitacionId);
                if (habitacion == null)
                {
                    _logger.LogWarning("Habitación no encontrada: {HabitacionId}", reserva.HabitacionId);
                    ModelState.AddModelError("HabitacionId", "La habitación seleccionada no existe.");
                    await CargarListasDesplegablesAsync(reserva.ClienteId, reserva.HabitacionId);
                    return View(reserva);
                }

                // Verificar disponibilidad
                var reservaExistente = await _context.Reservas
                    .AnyAsync(r =>
                        r.HabitacionId == reserva.HabitacionId && 
                        r.FechaEntrada.Date < reserva.FechaSalida.Date && 
                        r.FechaSalida.Date > reserva.FechaEntrada.Date);

                if (reservaExistente)
                {
                    _logger.LogWarning("Conflicto de reserva para habitación {HabitacionId}", reserva.HabitacionId);
                    ModelState.AddModelError("", "La habitación ya está reservada para las fechas seleccionadas.");
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

            if (ModelState.IsValid)
            {
                if (reserva.FechaSalida <= reserva.FechaEntrada)
                {
                    ModelState.AddModelError("FechaSalida", "La fecha de salida debe ser posterior a la fecha de entrada.");
                }

                // --- LÓGICA CORREGIDA ---
                // Comprueba si hay conflictos en la MISMA habitación, pero ignorando la reserva actual.
                var reservaExistente = await _context.Reservas
                    .AnyAsync(r =>
                        r.ReservaId != reserva.ReservaId &&        // Otra reserva
                        r.HabitacionId == reserva.HabitacionId && // En la misma habitación
                        r.FechaEntrada < reserva.FechaSalida &&   // Y las fechas se solapan
                        r.FechaSalida > reserva.FechaEntrada
                    );

                if (reservaExistente)
                {
                    ModelState.AddModelError(string.Empty, "El cambio de fechas entra en conflicto con otra reserva existente para esta habitación.");
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(reserva);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_context.Reservas.Any(e => e.ReservaId == reserva.ReservaId)) return NotFound();
                        else throw;
                    }
                    return RedirectToAction(nameof(Index));
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
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
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
    }
}