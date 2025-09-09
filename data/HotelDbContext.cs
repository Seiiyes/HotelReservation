using HotelReservations.Models;
using Microsoft.EntityFrameworkCore;

public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Habitacion> Habitaciones { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<ServicioAdicional> ServiciosAdicionales { get; set; }
    public DbSet<Pago> Pagos { get; set; }
}
