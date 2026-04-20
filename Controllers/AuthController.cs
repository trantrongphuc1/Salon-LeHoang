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
                if (User.IsInRole("Admin")) return RedirectToAction("Index", "Pos");
                // Khách hàng không có quyền truy cập hệ thống
                return RedirectToAction("Login");
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

            if (user.Role != "Admin")
            {
                ViewBag.Error = "Bạn không có quyền truy cập trang quản trị.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var identity = new ClaimsIdentity(claims, "UserAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("UserAuth", principal);

            return RedirectToAction("Index", "Pos");
        }



        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("UserAuth");
            return RedirectToAction("Login");
        }
    }
}
