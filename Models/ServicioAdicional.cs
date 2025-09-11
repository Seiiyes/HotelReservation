using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HotelReservations.Models
{
    [Table("ServiciosAdicionales")]
    public class ServicioAdicional
    {
        [Key]
        public int ServicioAdicionalId { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre del servicio")]
        public required string Nombre { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(5000, 500000, ErrorMessage = "El precio debe estar entre 5.000 y 500.000 COP")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio (COP)")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        public decimal Precio { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Fecha de solicitud")]
        [DataType(DataType.DateTime)]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Display(Name = "Estado del pago")]
        [StringLength(20)]
        [Required(ErrorMessage = "El estado del pago es requerido")]
        public required string EstadoPago { get; set; } = "Pendiente";

        [Required(ErrorMessage = "La reserva es requerida")]
        public int ReservaId { get; set; }

        [ForeignKey("ReservaId")]
        public virtual Reserva? Reserva { get; set; }
    }
}