using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Salon_LeHoang.Models;

namespace Salon_LeHoang.Controllers
{
    public class HomeController : Controller
    {
        private readonly SalonLeHoangContext _context;

        public HomeController(SalonLeHoangContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var services = _context.Services.Where(s => s.IsActive == true).ToList();
            return View(services);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
