using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Salon_LeHoang.Models;
using Salon_LeHoang.Models.ViewModels;

namespace Salon_LeHoang.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InvoiceController : Controller
    {
        private readonly SalonLeHoangContext _context;

        public InvoiceController(SalonLeHoangContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceDetails).ThenInclude(d => d.Service)
                .Include(i => i.InvoiceDetails).ThenInclude(d => d.Employee)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i => i.Customer.FullName.Contains(search) || i.Customer.PhoneNumber.Contains(search));
                ViewBag.Search = search;
            }

            if (fromDate.HasValue)
            {
                query = query.Where(i => i.PaymentDate >= fromDate.Value);
                ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            }

            if (toDate.HasValue)
            {
                query = query.Where(i => i.PaymentDate < toDate.Value.AddDays(1));
                ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            }

            var invoices = await query.OrderByDescending(i => i.PaymentDate).ToListAsync();
            return View(invoices);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _context.Users.Where(u => u.Role == "Customer" && u.IsActive).OrderBy(u => u.FullName).ToListAsync();
            ViewBag.Services = await _context.Services.Where(s => s.IsActive).OrderBy(s => s.ServiceName).ToListAsync();
            ViewBag.Employees = await _context.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int customerId, string paymentMethod, decimal pointsToUse, string? notes,
            int[] serviceIds, int[] employeeIds, int[] quantities)
        {
            var customer = await _context.Users.FindAsync(customerId);
            if (customer == null || customer.Role != "Customer")
            {
                TempData["Error"] = "Khách hàng không hợp lệ.";
                return RedirectToAction(nameof(Create));
            }

            if (serviceIds == null || serviceIds.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất 1 dịch vụ.";
                return RedirectToAction(nameof(Create));
            }

            // Tính tổng
            decimal totalAmount = 0;
            var details = new List<InvoiceDetail>();

            for (int i = 0; i < serviceIds.Length; i++)
            {
                var service = await _context.Services.FindAsync(serviceIds[i]);
                if (service == null) continue;

                var qty = (quantities != null && i < quantities.Length && quantities[i] > 0) ? quantities[i] : 1;
                var empId = (employeeIds != null && i < employeeIds.Length) ? employeeIds[i] : 0;

                if (empId == 0)
                {
                    TempData["Error"] = "Vui lòng chọn nhân viên cho mỗi dịch vụ.";
                    return RedirectToAction(nameof(Create));
                }

                var detail = new InvoiceDetail
                {
                    ServiceId = service.ServiceId,
                    EmployeeId = empId,
                    Price = service.Price,
                    Quantity = qty
                };

                totalAmount += service.Price * qty;
                details.Add(detail);
            }

            // Tính giảm giá từ điểm
            decimal discountAmount = 0;
            decimal actualPointsUsed = 0;
            if (pointsToUse > 0 && customer.Points > 0)
            {
                actualPointsUsed = Math.Min(pointsToUse, customer.Points);
                discountAmount = actualPointsUsed; // 1 điểm = 1đ
                if (discountAmount > totalAmount) 
                {
                    discountAmount = totalAmount;
                    actualPointsUsed = totalAmount;
                }
            }

            var finalAmount = totalAmount - discountAmount;
            var earnedPoints = Math.Round(finalAmount * 0.03m, 0); // 3% hóa đơn

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

            // Thêm chi tiết
            foreach (var d in details)
            {
                d.InvoiceId = invoice.InvoiceId;
                _context.InvoiceDetails.Add(d);
            }

            // Cập nhật điểm khách hàng
            customer.Points -= actualPointsUsed;
            customer.Points += earnedPoints;

            // Lưu lịch sử điểm
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

            TempData["Success"] = $"Tạo hóa đơn #{invoice.InvoiceId} thành công! Khách được cộng {earnedPoints} điểm.";
            return RedirectToAction(nameof(Details), new { id = invoice.InvoiceId });
        }

        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceDetails).ThenInclude(d => d.Service)
                .Include(i => i.InvoiceDetails).ThenInclude(d => d.Employee)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null) return NotFound();
            return View(invoice);
        }

        public async Task<IActionResult> Print(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.InvoiceDetails).ThenInclude(d => d.Service)
                .Include(i => i.InvoiceDetails).ThenInclude(d => d.Employee)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null) return NotFound();
            return View(invoice);
        }

        // API: Lấy thông tin khách hàng (dùng cho AJAX)
        [HttpGet]
        public async Task<IActionResult> GetCustomerInfo(int id)
        {
            var customer = await _context.Users.FindAsync(id);
            if (customer == null) return Json(new { success = false });
            return Json(new { success = true, name = customer.FullName, phone = customer.PhoneNumber, points = customer.Points });
        }
    }
}
