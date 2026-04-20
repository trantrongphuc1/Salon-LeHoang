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
            var yearStart = new DateTime(today.Year, 1, 1);

            var todayInvoices = await _context.Invoices
                .Include(i => i.InvoiceDetails)
                .Where(i => i.PaymentDate >= today)
                .ToListAsync();

            var monthInvoices = await _context.Invoices
                .Where(i => i.PaymentDate >= monthStart)
                .ToListAsync();

            var yearInvoices = await _context.Invoices
                .Where(i => i.PaymentDate >= yearStart)
                .ToListAsync();

            var totalCustomers = await _context.Users.CountAsync(u => u.Role == "Customer" && u.IsActive);
            var totalEmployees = await _context.Employees.CountAsync(e => e.IsActive);
            var totalServices = await _context.Services.CountAsync(s => s.IsActive);

            var recentInvoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceDetails).ThenInclude(d => d.Service)
                .OrderByDescending(i => i.PaymentDate)
                .Take(5)
                .ToListAsync();

            // Top Services
            var topServices = await _context.InvoiceDetails
                .Include(d => d.Service)
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

            // Top Customers
            var topCustomers = await _context.Invoices
                .Include(i => i.Customer)
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

            var viewModel = new DashboardViewModel
            {
                TodayRevenue = todayInvoices.Sum(i => i.FinalAmount),
                TodayInvoiceCount = todayInvoices.Count,
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
    }
}
