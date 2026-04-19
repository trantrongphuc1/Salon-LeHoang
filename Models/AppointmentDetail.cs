using System;
using System.Collections.Generic;

namespace Salon_LeHoang.Models;

public partial class AppointmentDetail
{
    public int AppointmentDetailId { get; set; }

    public int AppointmentId { get; set; }

    public int ServiceId { get; set; }

    public decimal Price { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
