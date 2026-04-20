using System;
using System.Collections.Generic;

namespace Salon_LeHoang.Models;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int CustomerId { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public int EarnedPoints { get; set; }

    public int PointsUsed { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? Notes { get; set; }

    public virtual User Customer { get; set; } = null!;

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

    public virtual ICollection<PointHistory> PointHistories { get; set; } = new List<PointHistory>();
}
