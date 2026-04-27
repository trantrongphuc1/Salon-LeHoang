using System;
using System.ComponentModel.DataAnnotations;

namespace Salon_LeHoang.Models
{
    public class Expense
    {
        [Key]
        public int ExpenseId { get; set; }

        [Required]
        [StringLength(200)]
        public string ExpenseName { get; set; } = null!;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
