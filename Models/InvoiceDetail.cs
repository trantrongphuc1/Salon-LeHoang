using System;

namespace Salon_LeHoang.Models;

public partial class InvoiceDetail
{
    public int InvoiceDetailId { get; set; }

    public int InvoiceId { get; set; }

    public int ServiceId { get; set; }

    public int EmployeeId { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
