using System.Collections.Generic;

namespace Salon_LeHoang.Models.ViewModels;

public class CreateInvoiceViewModel
{
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string PaymentMethod { get; set; } = "Tiền mặt";
    public int PointsToUse { get; set; }
    public string? Notes { get; set; }
    public List<InvoiceItemViewModel> Items { get; set; } = new List<InvoiceItemViewModel>();
}

public class InvoiceItemViewModel
{
    public int ServiceId { get; set; }
    public int EmployeeId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class DashboardViewModel
{
    public decimal TodayRevenue { get; set; }
    public int TodayInvoiceCount { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalServices { get; set; }
    public decimal MonthRevenue { get; set; }
    public int MonthInvoiceCount { get; set; }
    public List<Invoice> RecentInvoices { get; set; } = new List<Invoice>();
}

public class PayrollViewModel
{
    public int Month { get; set; }
    public int Year { get; set; }
    public List<EmployeePayrollItem> Items { get; set; } = new List<EmployeePayrollItem>();
}

public class EmployeePayrollItem
{
    public Employee Employee { get; set; } = null!;
    public int DaysOff { get; set; }
    public int AllowedDaysOff { get; set; } = 2;
    public int ExcessDaysOff => DaysOff > AllowedDaysOff ? DaysOff - AllowedDaysOff : 0;
    public int StandardWorkDays { get; set; } = 28;
    public int ActualWorkDays => StandardWorkDays - ExcessDaysOff;
    public decimal BaseSalaryEarned => ExcessDaysOff > 0
        ? Employee.BaseSalary / StandardWorkDays * ActualWorkDays
        : Employee.BaseSalary;
    public decimal TotalInvoiceAmount { get; set; }
    public decimal CommissionEarned => TotalInvoiceAmount * Employee.CommissionRate / 100;
    public decimal TotalSalary => BaseSalaryEarned + CommissionEarned;
    public string? AttendanceNotes { get; set; }
}

public class AttendanceViewModel
{
    public int Month { get; set; }
    public int Year { get; set; }
    public List<EmployeeAttendanceItem> Items { get; set; } = new List<EmployeeAttendanceItem>();
}

public class EmployeeAttendanceItem
{
    public Employee Employee { get; set; } = null!;
    public int DaysOff { get; set; }
    public string? Notes { get; set; }
    public bool IsViolation => DaysOff > 2;
}
