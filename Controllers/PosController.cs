using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Salon_LeHoang.Models;

namespace Salon_LeHoang.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PosController : Controller
    {
        private readonly SalonLeHoangContext _context;

        public PosController(SalonLeHoangContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.ServiceName)
                .ToListAsync();

            ViewBag.Employees = await _context.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.FullName)
                .ToListAsync();

            return View();
        }

        // API: Search customer by phone or name
        [HttpGet]
        public async Task<IActionResult> SearchCustomer(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<object>());

            var customers = await _context.Users
                .Where(u => u.Role == "Customer" && u.IsActive &&
                    (u.PhoneNumber.Contains(q) || u.FullName.Contains(q)))
                .Take(10)
                .Select(u => new { u.UserId, u.FullName, u.PhoneNumber, u.Points })
                .ToListAsync();

            return Json(customers);
        }

        // API: Quick-add customer
        [HttpPost]
        public async Task<IActionResult> QuickAddCustomer(string fullName, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(phoneNumber))
                return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin." });

            if (await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
                return Json(new { success = false, message = "Số điện thoại đã tồn tại." });

            var customer = new User
            {
                FullName = fullName,
                PhoneNumber = phoneNumber,
                PasswordHash = "customer123",
                Role = "Customer",
                Points = 0,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(customer);
            await _context.SaveChangesAsync();

            return Json(new { success = true, userId = customer.UserId, fullName = customer.FullName, phoneNumber = customer.PhoneNumber, points = 0 });
        }

        // API: Process payment
        [HttpPost]
        public async Task<IActionResult> Checkout(int customerId, string paymentMethod,
            int pointsToUse, string? notes, int[] serviceIds, int[] employeeIds, int[] quantities)
        {
            var customer = await _context.Users.FindAsync(customerId);
            if (customer == null || customer.Role != "Customer")
                return Json(new { success = false, message = "Khách hàng không hợp lệ." });

            if (serviceIds == null || serviceIds.Length == 0)
                return Json(new { success = false, message = "Chưa có dịch vụ nào." });

            decimal totalAmount = 0;
            var details = new List<InvoiceDetail>();

            for (int i = 0; i < serviceIds.Length; i++)
            {
                var service = await _context.Services.FindAsync(serviceIds[i]);
                if (service == null) continue;

                var qty = (quantities != null && i < quantities.Length && quantities[i] > 0) ? quantities[i] : 1;
                var empId = (employeeIds != null && i < employeeIds.Length) ? employeeIds[i] : 0;

                if (empId == 0)
                    return Json(new { success = false, message = $"Vui lòng chọn nhân viên cho dịch vụ: {service.ServiceName}" });

                details.Add(new InvoiceDetail
                {
                    ServiceId = service.ServiceId,
                    EmployeeId = empId,
                    Price = service.Price,
                    Quantity = qty
                });

                totalAmount += service.Price * qty;
            }

            // Points discount
            decimal discountAmount = 0;
            int actualPointsUsed = 0;
            if (pointsToUse > 0 && customer.Points > 0)
            {
                actualPointsUsed = Math.Min(pointsToUse, customer.Points);
                discountAmount = actualPointsUsed * 1000;
                if (discountAmount > totalAmount)
                {
                    discountAmount = totalAmount;
                    actualPointsUsed = (int)(totalAmount / 1000);
                }
            }

            var finalAmount = totalAmount - discountAmount;
            var earnedPoints = (int)(finalAmount / 10000);

            var invoice = new Invoice
            {
                CustomerId = customerId,
                TotalAmount = totalAmount,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                EarnedPoints = earnedPoints,
                PointsUsed = actualPointsUsed,
                PaymentMethod = paymentMethod ?? "Tiền mặt",
                PaymentDate = DateTime.Now,
                Notes = notes
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            foreach (var d in details)
            {
                d.InvoiceId = invoice.InvoiceId;
                _context.InvoiceDetails.Add(d);
            }

            // Update customer points
            customer.Points -= actualPointsUsed;
            customer.Points += earnedPoints;

            // Point history
            if (actualPointsUsed > 0)
            {
                _context.PointHistories.Add(new PointHistory
                {
                    UserId = customerId,
                    InvoiceId = invoice.InvoiceId,
                    PointsChanged = -actualPointsUsed,
                    Description = $"Sử dụng {actualPointsUsed} điểm - HĐ #{invoice.InvoiceId}",
                    CreatedAt = DateTime.Now
                });
            }

            if (earnedPoints > 0)
            {
                _context.PointHistories.Add(new PointHistory
                {
                    UserId = customerId,
                    InvoiceId = invoice.InvoiceId,
                    PointsChanged = earnedPoints,
                    Description = $"Tích {earnedPoints} điểm - HĐ #{invoice.InvoiceId}",
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                invoiceId = invoice.InvoiceId,
                totalAmount,
                discountAmount,
                finalAmount,
                earnedPoints,
                customerPoints = customer.Points
            });
        }
    }
}
