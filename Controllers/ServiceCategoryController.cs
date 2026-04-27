using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Salon_LeHoang.Models;

namespace Salon_LeHoang.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ServiceCategoryController : Controller
    {
        private readonly SalonLeHoangContext _context;

        public ServiceCategoryController(SalonLeHoangContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.ServiceCategories.ToListAsync();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceCategory category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ViewBag.Error = "Vui lòng nhập tên danh mục.";
                return View(category);
            }

            _context.ServiceCategories.Add(category);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Thêm danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.ServiceCategories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceCategory category)
        {
            var existing = await _context.ServiceCategories.FindAsync(category.CategoryId);
            if (existing == null) return NotFound();

            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ViewBag.Error = "Vui lòng nhập tên danh mục.";
                return View(category);
            }

            existing.CategoryName = category.CategoryName;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.ServiceCategories.FindAsync(id);
            if (category != null)
            {
                _context.ServiceCategories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa danh mục thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ManageServices(int id)
        {
            var category = await _context.ServiceCategories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound();

            var allServices = await _context.Services
                .Include(s => s.Category)
                .OrderBy(s => s.ServiceName)
                .ToListAsync();

            ViewBag.Category = category;
            return View(allServices);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageServices(int categoryId, List<int> selectedServiceIds)
        {
            // 1. Clear category from all services currently assigned to this category
            var currentServices = await _context.Services
                .Where(s => s.CategoryId == categoryId)
                .ToListAsync();

            foreach (var s in currentServices)
            {
                s.CategoryId = null;
            }

            // 2. Assign selected services to this category
            if (selectedServiceIds != null && selectedServiceIds.Count > 0)
            {
                var selected = await _context.Services
                    .Where(s => selectedServiceIds.Contains(s.ServiceId))
                    .ToListAsync();

                foreach (var s in selected)
                {
                    s.CategoryId = categoryId;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật dịch vụ thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
