using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Salon_LeHoang.Models;
using Salon_LeHoang.Models.ViewModels;

namespace Salon_LeHoang.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeController : Controller
    {
        private readonly SalonLeHoangContext _context;

        public EmployeeController(SalonLeHoangContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .Include(e => e.CategoryCommissions)
                .ThenInclude(c => c.Category)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
            return View(employees);
        }

        public async Task<IActionResult> Details(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.CategoryCommissions).ThenInclude(c => c.Category)
                .Include(e => e.InvoiceDetails).ThenInclude(d => d.Service)
                .Include(e => e.InvoiceDetails).ThenInclude(d => d.Invoice).ThenInclude(i => i.Customer)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return NotFound();
            return View(employee);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.ServiceCategories.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee, Dictionary<int, decimal> commissions)
        {
            if (string.IsNullOrWhiteSpace(employee.FullName))
            {
                ViewBag.Error = "Vui lòng nhập tên nhân viên.";
                ViewBag.Categories = await _context.ServiceCategories.ToListAsync();
                return View(employee);
            }

            employee.IsActive = true;
            employee.CreatedAt = DateTime.Now;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            if (commissions != null)
            {
                foreach (var kvp in commissions)
                {
                    _context.EmployeeCommissions.Add(new EmployeeCommission
                    {
                        EmployeeId = employee.EmployeeId,
                        CategoryId = kvp.Key,
                        CommissionRate = kvp.Value
                    });
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Thêm nhân viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.CategoryCommissions)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
            
            if (employee == null) return NotFound();
            ViewBag.Categories = await _context.ServiceCategories.ToListAsync();
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Employee employee, Dictionary<int, decimal> commissions)
        {
            var existing = await _context.Employees
                .Include(e => e.CategoryCommissions)
                .FirstOrDefaultAsync(e => e.EmployeeId == employee.EmployeeId);
            
            if (existing == null) return NotFound();

            if (string.IsNullOrWhiteSpace(employee.FullName))
            {
                ViewBag.Error = "Vui lòng nhập tên nhân viên.";
                ViewBag.Categories = await _context.ServiceCategories.ToListAsync();
                return View(employee);
            }

            existing.FullName = employee.FullName;
            existing.PhoneNumber = employee.PhoneNumber;
            existing.Position = employee.Position;
            existing.BaseSalary = employee.BaseSalary;
            existing.IsActive = employee.IsActive;

            // Update commissions
            _context.EmployeeCommissions.RemoveRange(existing.CategoryCommissions);
            if (commissions != null)
            {
                foreach (var kvp in commissions)
                {
                    _context.EmployeeCommissions.Add(new EmployeeCommission
                    {
                        EmployeeId = existing.EmployeeId,
                        CategoryId = kvp.Key,
                        CommissionRate = kvp.Value
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật nhân viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        // Bảng chấm công
        public async Task<IActionResult> Attendance(int? month, int? year)
        {
            var m = month ?? DateTime.Now.Month;
            var y = year ?? DateTime.Now.Year;

            var employees = await _context.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync();
            var attendances = await _context.Attendances
                .Where(a => a.AttendanceMonth == m && a.AttendanceYear == y)
                .ToListAsync();

            var vm = new AttendanceViewModel
            {
                Month = m,
                Year = y,
                Items = employees.Select(emp =>
                {
                    var att = attendances.FirstOrDefault(a => a.EmployeeId == emp.EmployeeId);
                    return new EmployeeAttendanceItem
                    {
                        Employee = emp,
                        DaysOff = att?.DaysOff ?? 0,
                        LateDays = att?.LateDays ?? 0,
                        Notes = att?.Notes,
                        LateNotes = att?.LateNotes
                    };
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAttendance(int employeeId, int month, int year, int daysOff, int lateDays, string? notes, string? lateNotes)
        {
            if (daysOff < 0) daysOff = 0;
            if (lateDays < 0) lateDays = 0;

            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceMonth == month && a.AttendanceYear == year);

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    EmployeeId = employeeId,
                    AttendanceMonth = month,
                    AttendanceYear = year,
                    DaysOff = daysOff,
                    LateDays = lateDays,
                    Notes = notes,
                    LateNotes = lateNotes,
                    CreatedAt = DateTime.Now
                };
                _context.Attendances.Add(attendance);
            }
            else
            {
                attendance.DaysOff = daysOff;
                attendance.LateDays = lateDays;
                attendance.Notes = notes;
                attendance.LateNotes = lateNotes;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật chấm công thành công!";
            return RedirectToAction(nameof(Attendance), new { month, year });
        }

        // Bảng lương
        public async Task<IActionResult> Payroll(int? month, int? year)
        {
            var m = month ?? DateTime.Now.Month;
            var y = year ?? DateTime.Now.Year;

            var employees = await _context.Employees
                .Include(e => e.CategoryCommissions)
                .Where(e => e.IsActive)
                .OrderBy(e => e.FullName)
                .ToListAsync();

            var attendances = await _context.Attendances
                .Where(a => a.AttendanceMonth == m && a.AttendanceYear == y)
                .ToListAsync();

            var startDate = new DateTime(y, m, 1);
            var endDate = startDate.AddMonths(1);

            var invoiceDetails = await _context.InvoiceDetails
                .Include(d => d.Invoice)
                .Include(d => d.Service)
                .Where(d => d.Invoice.PaymentDate >= startDate && d.Invoice.PaymentDate < endDate)
                .ToListAsync();

            var categories = await _context.ServiceCategories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Categories = categories;

            var vm = new PayrollViewModel
            {
                Month = m,
                Year = y,
                Items = employees.Select(emp =>
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
                        
                        var rate = emp.CategoryCommissions
                            .FirstOrDefault(c => c.CategoryId == cat.CategoryId)?.CommissionRate ?? 0;

                        catTotals[cat.CategoryId] = totalForCat;
                        catComms[cat.CategoryId] = totalForCat * rate / 100;
                    }

                    return new EmployeePayrollItem
                    {
                        Employee = emp,
                        DaysOff = att?.DaysOff ?? 0,
                        LateDays = att?.LateDays ?? 0,
                        CategoryTotals = catTotals,
                        CategoryCommissions = catComms,
                        AttendanceNotes = att?.Notes,
                        LateNotes = att?.LateNotes
                    };
                }).ToList()
            };

            return View(vm);
        }

        // Chi tiết lương 1 nhân viên
        public async Task<IActionResult> PayrollDetail(int employeeId, int? month, int? year)
        {
            var m = month ?? DateTime.Now.Month;
            var y = year ?? DateTime.Now.Year;

            var employee = await _context.Employees
                .Include(e => e.CategoryCommissions)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null) return NotFound();

            var att = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceMonth == m && a.AttendanceYear == y);

            var startDate = new DateTime(y, m, 1);
            var endDate = startDate.AddMonths(1);

            var invoiceDetails = await _context.InvoiceDetails
                .Include(d => d.Invoice).ThenInclude(i => i.Customer)
                .Include(d => d.Service).ThenInclude(s => s.Category)
                .Where(d => d.EmployeeId == employeeId && d.Invoice.PaymentDate >= startDate && d.Invoice.PaymentDate < endDate)
                .OrderBy(d => d.Invoice.PaymentDate)
                .ToListAsync();

            var categories = await _context.ServiceCategories.OrderBy(c => c.CategoryName).ToListAsync();
            ViewBag.Categories = categories;

            var catTotals = new Dictionary<int, decimal>();
            var catComms = new Dictionary<int, decimal>();

            foreach (var cat in categories)
            {
                var totalForCat = invoiceDetails
                    .Where(d => d.Service.CategoryId == cat.CategoryId)
                    .Sum(d => d.Price * d.Quantity);

                var rate = employee.CategoryCommissions
                    .FirstOrDefault(c => c.CategoryId == cat.CategoryId)?.CommissionRate ?? 0;

                catTotals[cat.CategoryId] = totalForCat;
                catComms[cat.CategoryId] = totalForCat * rate / 100;
            }

            var payrollItem = new EmployeePayrollItem
            {
                Employee = employee,
                DaysOff = att?.DaysOff ?? 0,
                LateDays = att?.LateDays ?? 0,
                CategoryTotals = catTotals,
                CategoryCommissions = catComms,
                AttendanceNotes = att?.Notes,
                LateNotes = att?.LateNotes
            };

            ViewBag.InvoiceDetails = invoiceDetails;
            ViewBag.Month = m;
            ViewBag.Year = y;

            return View(payrollItem);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                employee.IsActive = !employee.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
