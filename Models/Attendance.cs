using System;

namespace Salon_LeHoang.Models;

public partial class Attendance
{
    public int AttendanceId { get; set; }

    public int EmployeeId { get; set; }

    public int AttendanceMonth { get; set; }

    public int AttendanceYear { get; set; }

    public int DaysOff { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
