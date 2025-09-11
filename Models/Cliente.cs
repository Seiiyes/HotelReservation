using System.ComponentModel.DataAnnotations;

namespace HotelReservations.Models;

public class Cliente
{
    [Key]
    public int ClienteId { get; set; }

    [Required, MaxLength(20)]
    public string DocumentoIdentidad { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Apellido { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;


    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
