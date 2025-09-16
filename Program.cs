using HotelReservations.Models; // Asegúrate de que la ruta a tu DbContext es correcta
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Se obtiene la cadena de conexión. Es importante validar que no sea nula o vacía.
var connectionString = builder.Configuration.GetConnectionString("HotelDBConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Lanzar una excepción si no se encuentra la cadena de conexión es una buena práctica
    // para fallar rápido y evitar errores inesperados más adelante.
    throw new InvalidOperationException("La cadena de conexión 'HotelDBConnection' no se encontró en la configuración.");
}

builder.Services.AddDbContext<HotelDbContext>(options =>
    // Usar AutoDetect para la versión del servidor MySQL hace que la aplicación sea más portable
    // y no depende de una versión específica hardcodeada.
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 1. Añadir los servicios de Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        // Configuración de requisitos de contraseña
        options.Password.RequireDigit = true;           // Requerir al menos un número
        options.Password.RequiredLength = 8;            // Longitud mínima de 8 caracteres
        options.Password.RequireNonAlphanumeric = true; // Requerir al menos un caracter especial (ej. @, #, $)
        options.Password.RequireUppercase = true;       // Requerir al menos una mayúscula
        options.Password.RequireLowercase = true;       // Requerir al menos una minúscula
    })
    .AddRoles<IdentityRole>() // <-- Añadir esta línea para habilitar la gestión de roles
    .AddEntityFrameworkStores<HotelDbContext>();

// 2. Añadir soporte para Razor Pages (necesario para las vistas de Identity)
builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. Añadir el middleware de autenticación y autorización (¡el orden es importante!)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 4. Mapear las Razor Pages de Identity
app.MapRazorPages();

app.Run();