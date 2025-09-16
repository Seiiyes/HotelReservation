using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HotelReservations.Models
{
    public class ReportePagosViewModel
    {
        [Display(Name = "Fecha de Inicio")]
        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; }

        [Display(Name = "Fecha de Fin")]
        [DataType(DataType.Date)]
        public DateTime? FechaFin { get; set; }

        [Display(Name = "Método de Pago")]
        public string? MetodoPago { get; set; }

        public SelectList? MetodosDePago { get; set; }

        public List<Pago>? Resultados { get; set; }

        public decimal TotalRecaudado => Resultados?.Sum(p => p.Monto) ?? 0;
    }
}