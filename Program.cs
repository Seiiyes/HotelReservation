using HotelReservations.Models; // Asegúrate de que la ruta a tu DbContext es correcta
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("HotelDBConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("ERROR: La cadena de conexión 'HotelDBConnection' no se pudo encontrar en appsettings.json. Verifica que el nombre sea correcto y que las propiedades del archivo estén en 'Copy if newer'.");
}


builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 33))));

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