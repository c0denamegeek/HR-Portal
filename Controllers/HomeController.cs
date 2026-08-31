using HR_Portal.Constants;
using HR_Portal.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HR_Portal.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated ?? true)
                return RedirectToAction("Login", "Account");

            if (User.IsInRole(Roles.Admin))
                return RedirectToAction("Dashboard", "AdminDashboard");

            // User role — manager flag is handled inside the dashboard
            return RedirectToAction("Dashboard", "Leave");
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
