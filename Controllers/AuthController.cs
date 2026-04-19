using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Salon_LeHoang.Models;
using System.Security.Claims;

namespace Salon_LeHoang.Controllers
{
    public class AuthController : Controller
    {
        private readonly SalonLeHoangContext _context;

        public AuthController(SalonLeHoangContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
                return RedirectToAction("Index", "Customer");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string phoneNumber, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber && u.PasswordHash == password);
            if (user == null)
            {
                ViewBag.Error = "Số điện thoại hoặc mật khẩu không đúng.";
                return View();
            }

            if (user.IsActive == false)
            {
                ViewBag.Error = "Tài khoản của bạn đã bị khóa.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.Role, user.Role ?? "Customer")
            };

            var identity = new ClaimsIdentity(claims, "UserAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("UserAuth", principal);

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Admin");
                
            return RedirectToAction("Index", "Customer");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string fullName, string phoneNumber, string password)
        {
            if (_context.Users.Any(u => u.PhoneNumber == phoneNumber))
            {
                ViewBag.Error = "Số điện thoại đã được đăng ký.";
                return View();
            }

            var user = new User
            {
                FullName = fullName,
                PhoneNumber = phoneNumber,
                PasswordHash = password, // In production, hash the password
                Role = "Customer",
                Points = 0,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("UserAuth");
            return RedirectToAction("Login");
        }
    }
}
