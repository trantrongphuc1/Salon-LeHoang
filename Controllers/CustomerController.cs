using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Salon_LeHoang.Models;

namespace Salon_LeHoang.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly SalonLeHoangContext _context;

        public CustomerController(SalonLeHoangContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Users.Where(u => u.Role == "Customer").AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || u.PhoneNumber.Contains(search));
                ViewBag.Search = search;
            }

            var customers = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(customers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User customer)
        {
            if (string.IsNullOrWhiteSpace(customer.FullName) || string.IsNullOrWhiteSpace(customer.PhoneNumber))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View(customer);
            }

            // Check duplicate phone
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == customer.PhoneNumber))
            {
                ViewBag.Error = "Số điện thoại đã tồn tại trong hệ thống.";
                return View(customer);
            }

            customer.Role = "Customer";
            customer.PasswordHash = "customer123"; // default password
            customer.Points = 0;
            customer.IsActive = true;
            customer.CreatedAt = DateTime.Now;

            _context.Users.Add(customer);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm khách hàng thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Users.FindAsync(id);
            if (customer == null || customer.Role != "Customer") return NotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User customer)
        {
            var existing = await _context.Users.FindAsync(customer.UserId);
            if (existing == null || existing.Role != "Customer") return NotFound();

            if (string.IsNullOrWhiteSpace(customer.FullName))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View(customer);
            }

            existing.FullName = customer.FullName;
            existing.PhoneNumber = customer.PhoneNumber;
            existing.IsActive = customer.IsActive;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật khách hàng thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _context.Users
                .Include(u => u.Invoices)
                    .ThenInclude(i => i.InvoiceDetails)
                        .ThenInclude(d => d.Service)
                .Include(u => u.PointHistories)
                .FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Customer");

            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> AdjustPoints(int userId, int points, string reason)
        {
            var customer = await _context.Users.FindAsync(userId);
            if (customer == null || customer.Role != "Customer") return NotFound();

            customer.Points += points;
            if (customer.Points < 0) customer.Points = 0;

            _context.PointHistories.Add(new PointHistory
            {
                UserId = userId,
                PointsChanged = points,
                Description = reason ?? (points > 0 ? "Cộng điểm thủ công" : "Trừ điểm thủ công"),
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã điều chỉnh {points} điểm cho khách hàng.";
            return RedirectToAction(nameof(Details), new { id = userId });
        }
    }
}
