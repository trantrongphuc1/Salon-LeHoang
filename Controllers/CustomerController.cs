using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Salon_LeHoang.Models;
using System.Security.Claims;

namespace Salon_LeHoang.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly SalonLeHoangContext _context;

        public CustomerController(SalonLeHoangContext context)
        {
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");

            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            var appointments = await _context.Appointments
                .Include(a => a.AppointmentDetails)
                .ThenInclude(ad => ad.Service)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(10)
                .ToListAsync();

            ViewBag.Points = user?.Points ?? 0;
            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> Booking()
        {
            var services = await _context.Services.Where(s => s.IsActive == true).ToListAsync();
            return View(services);
        }

        [HttpPost]
        public async Task<IActionResult> Booking(List<int> serviceIds, DateTime appointmentDate, string notes)
        {
            if (serviceIds == null || !serviceIds.Any())
            {
                ViewBag.Error = "Vui lòng chọn ít nhất một dịch vụ.";
                var services = await _context.Services.Where(s => s.IsActive == true).ToListAsync();
                return View(services);
            }

            var userId = GetUserId();
            var appointment = new Appointment
            {
                UserId = userId,
                AppointmentDate = appointmentDate,
                Notes = notes,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var selectedServices = await _context.Services.Where(s => serviceIds.Contains(s.ServiceId)).ToListAsync();
            foreach (var service in selectedServices)
            {
                _context.AppointmentDetails.Add(new AppointmentDetail
                {
                    AppointmentId = appointment.AppointmentId,
                    ServiceId = service.ServiceId,
                    Price = service.Price
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đặt lịch thành công! Chúng tôi sẽ liên hệ lại với bạn.";
            return RedirectToAction(nameof(Index));
        }
    }
}
