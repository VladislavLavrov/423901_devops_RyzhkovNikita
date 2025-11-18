using System;
using System.ComponentModel.DataAnnotations;

namespace CalculatorApp.Models
{
    public class CalculationHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public double Operand1 { get; set; }

        [Required]
        public string Operation { get; set; } = string.Empty;

        public double? Operand2 { get; set; }

        [Required]
        public double Result { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}