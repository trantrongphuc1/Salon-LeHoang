using System;
using System.Collections.Generic;

namespace Salon_LeHoang.Models;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int AppointmentId { get; set; }

    public decimal TotalAmount { get; set; }

    public int EarnedPoints { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? Notes { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;
}
