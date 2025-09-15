using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HotelReservations.Models;
using Microsoft.AspNetCore.Authorization;


namespace HotelReservations.Controllers;

// Al añadir [Authorize] al controlador, todas sus acciones requerirán que el usuario
// haya iniciado sesión, a menos que se indique lo contrario con [AllowAnonymous].
[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // Esta es ahora la página principal (dashboard) para usuarios autenticados.
    public IActionResult Index()
    {
        return View();
    }

    // La página de error debe ser accesible para todos, por eso se añade [AllowAnonymous].
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}