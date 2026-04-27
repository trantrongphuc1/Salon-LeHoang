using System.ComponentModel.DataAnnotations;

namespace Salon_LeHoang.Models
{
    public class EmployeeCommission
    {
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        public int CategoryId { get; set; }
        public virtual ServiceCategory Category { get; set; } = null!;

        [Range(0, 100)]
        public decimal CommissionRate { get; set; }
    }
}
