using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservations.Models
{
    public class Habitacion
    {
          [Key]
    public int HabitacionId { get; set; }
    public string Tipo { get; set; }
    public int Capacidad { get; set; }
    public decimal Precio { get; set; }
    public bool Disponible { get; set; }

    public ICollection<Reserva> Reservas { get; set; }
    }
}