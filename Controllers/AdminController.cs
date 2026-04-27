using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Salon_LeHoang.Models;
using Salon_LeHoang.Models.ViewModels;

namespace Salon_LeHoang.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SalonLeHoangContext _context;

        public AdminController(SalonLeHoangContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? date)
        {
            var isDailyView = date.HasValue;
            var targetDate = date ?? DateTime.Today;
            var nextDay = targetDate.AddDays(1);
            
            var monthStart = new DateTime(targetDate.Year, targetDate.Month, 1);
            var yearStart = new DateTime(targetDate.Year, 1, 1);

            // Doanh thu ngày được chọn (hoặc hôm nay)
            var targetDateInvoices = await _context.Invoices
                .Include(i => i.InvoiceDetails)
                .Where(i => i.PaymentDate >= targetDate && i.PaymentDate < nextDay)
                .ToListAsync();

            var monthInvoices = await _context.Invoices
                .Where(i => i.PaymentDate >= monthStart && i.PaymentDate < monthStart.AddMonths(1))
                .ToListAsync();

            var yearInvoices = await _context.Invoices
                .Where(i => i.PaymentDate >= yearStart && i.PaymentDate < yearStart.AddYears(1))
                .ToListAsync();

            var totalCustomers = await _context.Users.CountAsync(u => u.Role == "Customer" && u.IsActive);
            var totalEmployees = await _context.Employees.CountAsync(e => e.IsActive);
            var totalServices = await _context.Services.CountAsync(s => s.IsActive);

            // Recent Invoices: Filter by day if in Daily View, otherwise last 5 overall
            var recentInvoicesQuery = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceDetails).ThenInclude(d => d.Service)
                .OrderByDescending(i => i.PaymentDate);

            List<Invoice> recentInvoices;
            if (isDailyView)
            {
                recentInvoices = await recentInvoicesQuery
                    .Where(i => i.PaymentDate >= targetDate && i.PaymentDate < nextDay)
                    .ToListAsync();
            }
            else
            {
                recentInvoices = await recentInvoicesQuery
                    .Take(5)
                    .ToListAsync();
            }

            // Top Services & Customers: Filter by day if in Daily View, otherwise by month
            var statsStartDate = isDailyView ? targetDate : monthStart;
            var statsEndDate = isDailyView ? nextDay : monthStart.AddMonths(1);

            var topServices = await _context.InvoiceDetails
                .Include(d => d.Service)
                .Include(d => d.Invoice)
                .Where(d => d.Invoice.PaymentDate >= statsStartDate && d.Invoice.PaymentDate < statsEndDate)
                .GroupBy(d => d.Service.ServiceName)
                .Select(g => new TopServiceViewModel
                {
                    ServiceName = g.Key,
                    UsageCount = g.Sum(d => d.Quantity),
                    TotalRevenue = g.Sum(d => d.Price * d.Quantity)
                })
                .OrderByDescending(x => x.UsageCount)
                .Take(5)
                .ToListAsync();

            var topCustomers = await _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.PaymentDate >= statsStartDate && i.PaymentDate < statsEndDate)
                .GroupBy(i => new { i.Customer.FullName, i.Customer.PhoneNumber })
                .Select(g => new TopCustomerViewModel
                {
                    FullName = g.Key.FullName,
                    PhoneNumber = g.Key.PhoneNumber,
                    InvoiceCount = g.Count(),
                    TotalSpent = g.Sum(i => i.FinalAmount)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(5)
                .ToListAsync();

            ViewBag.SelectedDate = date; // Can be null
            ViewBag.IsDailyView = isDailyView;
            ViewBag.DisplayDate = targetDate;

            var viewModel = new DashboardViewModel
            {
                TodayRevenue = targetDateInvoices.Sum(i => i.FinalAmount),
                TodayInvoiceCount = targetDateInvoices.Count,
                MonthRevenue = monthInvoices.Sum(i => i.FinalAmount),
                MonthInvoiceCount = monthInvoices.Count,
                YearRevenue = yearInvoices.Sum(i => i.FinalAmount),
                TotalCustomers = totalCustomers,
                TotalEmployees = totalEmployees,
                TotalServices = totalServices,
                RecentInvoices = recentInvoices,
                TopServices = topServices,
                TopCustomers = topCustomers
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ProfitReport(int? month, int? year)
        {
            try
            {
                var m = month ?? DateTime.Now.Month;
                var y = year ?? DateTime.Now.Year;

                var startDate = new DateTime(y, m, 1);
                var endDate = startDate.AddMonths(1);

                // 1. Doanh thu
                var totalRevenue = await _context.Invoices
                    .Where(i => i.PaymentDate >= startDate && i.PaymentDate < endDate)
                    .SumAsync(i => i.FinalAmount);

                // 2. Lương nhân viên
                var employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
                var attendances = await _context.Attendances
                    .Where(a => a.AttendanceMonth == m && a.AttendanceYear == y)
                    .ToListAsync();

                var invoiceDetails = await _context.InvoiceDetails
                    .Include(d => d.Invoice)
                    .Include(d => d.Service)
                    .Where(d => d.Invoice.PaymentDate >= startDate && d.Invoice.PaymentDate < endDate)
                    .ToListAsync();

                var categories = await _context.ServiceCategories.OrderBy(c => c.CategoryName).ToListAsync();

                decimal totalSalaries = 0;
                foreach (var emp in employees)
                {
                    var att = attendances.FirstOrDefault(a => a.EmployeeId == emp.EmployeeId);
                    var empDetails = invoiceDetails.Where(x => x.EmployeeId == emp.EmployeeId);

                    var catTotals = new Dictionary<int, decimal>();
                    var catComms = new Dictionary<int, decimal>();

                    foreach (var cat in categories)
                    {
                        var totalForCat = empDetails
                            .Where(d => d.Service.CategoryId == cat.CategoryId)
                            .Sum(d => d.Price * d.Quantity);

                        var rate = _context.EmployeeCommissions
                            .FirstOrDefault(c => c.EmployeeId == emp.EmployeeId && c.CategoryId == cat.CategoryId)?.CommissionRate ?? 0;

                        catTotals[cat.CategoryId] = totalForCat;
                        catComms[cat.CategoryId] = totalForCat * rate / 100;
                    }

                    var payrollItem = new EmployeePayrollItem
                    {
                        Employee = emp,
                        DaysOff = att?.DaysOff ?? 0,
                        LateDays = att?.LateDays ?? 0,
                        CategoryTotals = catTotals,
                        CategoryCommissions = catComms
                    };
                    totalSalaries += payrollItem.TotalSalary;
                }

                // 3. Chi phí khác
                var expenses = await _context.Expenses
                    .Where(e => e.Month == m && e.Year == y)
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                var viewModel = new ProfitReportViewModel
                {
                    Month = m,
                    Year = y,
                    TotalRevenue = totalRevenue,
                    TotalSalaries = totalSalaries,
                    OtherExpenses = expenses
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải báo cáo. Có thể bạn chưa khởi tạo bảng Expenses trong cơ sở dữ liệu. Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddExpense(string name, decimal amount, int month, int year)
        {
            if (string.IsNullOrWhiteSpace(name) || amount <= 0)
            {
                TempData["Error"] = "Vui lòng nhập tên chi phí và số tiền hợp lệ.";
                return RedirectToAction(nameof(ProfitReport), new { month, year });
            }

            var expense = new Expense
            {
                ExpenseName = name,
                Amount = amount,
                Month = month,
                Year = year,
                CreatedAt = DateTime.Now
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã thêm chi phí.";
            return RedirectToAction(nameof(ProfitReport), new { month, year });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                int m = expense.Month;
                int y = expense.Year;
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa chi phí.";
                return RedirectToAction(nameof(ProfitReport), new { month = m, year = y });
            }
            return RedirectToAction(nameof(ProfitReport));
        }
    }
}
