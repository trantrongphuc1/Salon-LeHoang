using System;

namespace Salon_LeHoang.Models;

public partial class PointHistory
{
    public int HistoryId { get; set; }

    public int UserId { get; set; }

    public int? InvoiceId { get; set; }

    public int PointsChanged { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Invoice? Invoice { get; set; }
}
