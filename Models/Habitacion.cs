using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelReservations.Models
{
    public class Habitacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HabitacionId { get; set; }

        [Required(ErrorMessage = "El tipo de habitación es obligatorio")]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [Range(1, 20, ErrorMessage = "La capacidad debe ser entre 1 y 20")]
        public int Capacidad { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(typeof(decimal), "50000", "2000000", ErrorMessage = "El precio debe estar entre 50.000 y 2.000.000 COP")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Precio (COP)")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = false)]
        public decimal Precio { get; set; }

        public bool Disponible { get; set; }

       public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    }
}
