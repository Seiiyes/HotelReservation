using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservations.Models
{
    public class Pago
    {
        [Key]
    public int PagoId { get; set; }

    public int ReservaId { get; set; }
    public Reserva Reserva { get; set; }

    public DateTime FechaPago { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; }
    }
}