using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservations.Models
{
    public class Reserva
    {
        [Key]
    public int ReservaId { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; }

    public int HabitacionId { get; set; }
    public Habitacion Habitacion { get; set; }

    public DateTime FechaEntrada { get; set; }
    public DateTime FechaSalida { get; set; }

    public ICollection<ServicioAdicional> Servicios { get; set; }
    public ICollection<Pago> Pagos { get; set; }
    }
}