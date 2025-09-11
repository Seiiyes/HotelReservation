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
        public string Tipo { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "La capacidad debe ser entre 1 y 20")]
        public int Capacidad { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(typeof(decimal), "0,01", "100000,00", ErrorMessage = "El precio debe ser mayor a 0")]
        [Column(TypeName = "decimal(18,2)")] // <- Muy importante
        public decimal Precio { get; set; }

        public bool Disponible { get; set; }

       public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    }
}
