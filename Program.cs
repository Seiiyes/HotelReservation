using HotelReservations.Models; // Asegúrate de que la ruta a tu DbContext es correcta
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("HotelDBConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("ERROR: La cadena de conexión 'HotelDBConnection' no se pudo encontrar en appsettings.json. Verifica que el nombre sea correcto y que las propiedades del archivo estén en 'Copy if newer'.");
}


builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 33))));


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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();