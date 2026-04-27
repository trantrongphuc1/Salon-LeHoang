using System.Collections.Generic;

namespace Salon_LeHoang.Models.ViewModels;

public class CreateInvoiceViewModel
{
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string PaymentMethod { get; set; } = "Tiền mặt";
    public decimal PointsToUse { get; set; }
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
    public decimal YearRevenue { get; set; }
    public List<Invoice> RecentInvoices { get; set; } = new List<Invoice>();
    public List<TopServiceViewModel> TopServices { get; set; } = new List<TopServiceViewModel>();
    public List<TopCustomerViewModel> TopCustomers { get; set; } = new List<TopCustomerViewModel>();
}

public class TopServiceViewModel
{
    public string ServiceName { get; set; } = "";
    public int UsageCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class TopCustomerViewModel
{
    public string FullName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public int InvoiceCount { get; set; }
    public decimal TotalSpent { get; set; }
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
    public int LateDays { get; set; }
    public int AllowedDaysOff { get; set; } = 2;
    public int ExcessDaysOff => DaysOff > AllowedDaysOff ? DaysOff - AllowedDaysOff : 0;
    public int StandardWorkDays { get; set; } = 28;
    public int ActualWorkDays => StandardWorkDays - ExcessDaysOff;
    public decimal BaseSalaryEarned => ExcessDaysOff > 0
        ? Employee.BaseSalary / StandardWorkDays * ActualWorkDays
        : Employee.BaseSalary;

    // Dictionary Key: CategoryId, Value: Total Amount
    public Dictionary<int, decimal> CategoryTotals { get; set; } = new Dictionary<int, decimal>();
    
    // Dictionary Key: CategoryId, Value: Commission Earned
    public Dictionary<int, decimal> CategoryCommissions { get; set; } = new Dictionary<int, decimal>();

    public decimal TotalCommission => CategoryCommissions.Values.Sum();
    public decimal TotalSalary => BaseSalaryEarned + TotalCommission;
    public string? AttendanceNotes { get; set; }
    public string? LateNotes { get; set; }
    public bool IsViolation => DaysOff > 2 || LateDays > 5;
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
    public int LateDays { get; set; }
    public string? Notes { get; set; }
    public string? LateNotes { get; set; }
    public bool IsViolation => DaysOff > 2 || LateDays > 5;
}

public class ProfitReportViewModel
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalSalaries { get; set; }
    public List<Expense> OtherExpenses { get; set; } = new List<Expense>();
    public decimal TotalOtherExpenses => OtherExpenses.Sum(e => e.Amount);
    public decimal NetProfit => TotalRevenue - TotalSalaries - TotalOtherExpenses;
}
