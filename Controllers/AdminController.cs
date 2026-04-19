using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Salon_LeHoang.Models;

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
            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.AppointmentDetails)
                .ThenInclude(ad => ad.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int appointmentId, string status)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment != null)
            {
                appointment.Status = status;
                appointment.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật trạng thái thành công.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CompleteAndInvoice(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.AppointmentDetails)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment != null && appointment.Status != "Completed")
            {
                var totalAmount = appointment.AppointmentDetails.Sum(ad => ad.Price);
                var earnedPoints = (int)(totalAmount / 10000);

                var invoice = new Invoice
                {
                    AppointmentId = appointment.AppointmentId,
                    TotalAmount = totalAmount,
                    EarnedPoints = earnedPoints,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = "Cash",
                    Notes = "Thanh toán dịch vụ"
                };

                _context.Invoices.Add(invoice);

                appointment.Status = "Completed";
                appointment.UpdatedAt = DateTime.Now;
                appointment.User.Points += earnedPoints;

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Hoàn tất dịch vụ. Khách hàng được cộng {earnedPoints} điểm.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
