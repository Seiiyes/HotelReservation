using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; 

namespace HotelReservations.Models
{
    public class Reserva
    {
        [Key]
        public int ReservaId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [Display(Name = "Cliente")]
        public virtual Cliente? Cliente { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una habitación.")]
        [Display(Name = "Habitación")]
        public int HabitacionId { get; set; }

        [Display(Name = "Habitación")]
        public virtual Habitacion? Habitacion { get; set; }

        [Required(ErrorMessage = "La fecha de entrada es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Entrada")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FechaEntrada { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Salida")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [CustomValidation(typeof(Reserva), nameof(ValidarFechaSalida))]
        public DateTime FechaSalida { get; set; } = DateTime.Today.AddDays(1);

        public virtual ICollection<ServicioAdicional> Servicios { get; set; } = new List<ServicioAdicional>();
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        public static ValidationResult? ValidarFechaSalida(DateTime fechaSalida, ValidationContext context)
        {
            var reserva = (Reserva)context.ObjectInstance;
            if (fechaSalida <= reserva.FechaEntrada)
            {
                return new ValidationResult("La fecha de salida debe ser posterior a la fecha de entrada.");
            }
            return ValidationResult.Success;
        }
    }
}