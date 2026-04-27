using System;
using System.Collections.Generic;

namespace Salon_LeHoang.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string FullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Position { get; set; }

    public decimal BaseSalary { get; set; }

    public virtual ICollection<EmployeeCommission> CategoryCommissions { get; set; } = new List<EmployeeCommission>();

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
}
