using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservations.Models
{
    public class ServicioAdicional
    {
        
    [Key]
    public int ServicioId { get; set; }
    public string Nombre { get; set; }
    public decimal Precio { get; set; }

    public int ReservaId { get; set; }
    public Reserva Reserva { get; set; }
    }
}