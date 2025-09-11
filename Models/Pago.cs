using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservations.Models
{
    public class Pago
    {
        [Key]
    public int PagoId { get; set; }

    [Required(ErrorMessage = "La reserva es requerida")]
    public int ReservaId { get; set; }
    public required virtual Reserva Reserva { get; set; }

    [Required(ErrorMessage = "La fecha de pago es requerida")]
    [Display(Name = "Fecha de pago")]
    public DateTime FechaPago { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "El monto es requerido")]
    [Range(1, 100000000, ErrorMessage = "El monto debe estar entre $1 y $100,000,000")]
    [Column(TypeName = "decimal(18,0)")]
    [Display(Name = "Monto (COP)")]
    [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "El método de pago es requerido")]
    [Display(Name = "Método de pago")]
    [StringLength(50)]
    public required string MetodoPago { get; set; }
    }
}