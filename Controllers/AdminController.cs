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

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var vm = new DashboardViewModel
            {
                TodayRevenue = await _context.Invoices
                    .Where(i => i.PaymentDate >= today && i.PaymentDate < today.AddDays(1))
                    .SumAsync(i => (decimal?)i.FinalAmount) ?? 0,

                TodayInvoiceCount = await _context.Invoices
                    .CountAsync(i => i.PaymentDate >= today && i.PaymentDate < today.AddDays(1)),

                TotalCustomers = await _context.Users.CountAsync(u => u.Role == "Customer" && u.IsActive),

                TotalEmployees = await _context.Employees.CountAsync(e => e.IsActive),

                TotalServices = await _context.Services.CountAsync(s => s.IsActive),

                MonthRevenue = await _context.Invoices
                    .Where(i => i.PaymentDate >= monthStart && i.PaymentDate < monthEnd)
                    .SumAsync(i => (decimal?)i.FinalAmount) ?? 0,

                MonthInvoiceCount = await _context.Invoices
                    .CountAsync(i => i.PaymentDate >= monthStart && i.PaymentDate < monthEnd),

                RecentInvoices = await _context.Invoices
                    .Include(i => i.Customer)
                    .Include(i => i.InvoiceDetails).ThenInclude(d => d.Service)
                    .OrderByDescending(i => i.PaymentDate)
                    .Take(10)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
