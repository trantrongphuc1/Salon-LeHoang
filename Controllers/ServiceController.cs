using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Salon_LeHoang.Models;

namespace Salon_LeHoang.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServiceController : Controller
    {
        private readonly SalonLeHoangContext _context;

        public ServiceController(SalonLeHoangContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Services.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.ServiceName.Contains(search));
                ViewBag.Search = search;
            }

            var services = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
            return View(services);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (string.IsNullOrWhiteSpace(service.ServiceName) || service.Price <= 0)
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View(service);
            }

            service.IsActive = true;
            service.CreatedAt = DateTime.Now;
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm dịch vụ thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Service service)
        {
            var existing = await _context.Services.FindAsync(service.ServiceId);
            if (existing == null) return NotFound();

            if (string.IsNullOrWhiteSpace(service.ServiceName) || service.Price <= 0)
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View(service);
            }

            existing.ServiceName = service.ServiceName;
            existing.Description = service.Description;
            existing.Price = service.Price;
            existing.DurationMinutes = service.DurationMinutes;
            existing.ImageUrl = service.ImageUrl;
            existing.IsActive = service.IsActive;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật dịch vụ thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                service.IsActive = !service.IsActive;
                await _context.SaveChangesAsync();
                TempData["Success"] = service.IsActive ? "Đã kích hoạt dịch vụ." : "Đã tắt dịch vụ.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                service.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa dịch vụ.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
