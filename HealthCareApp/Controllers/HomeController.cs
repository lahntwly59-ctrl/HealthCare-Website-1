using Microsoft.AspNetCore.Mvc;

namespace HealthCareApp.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // Must be logged in
            if (HttpContext.Session.GetInt32("PatientId") == null)
                return RedirectToAction("Login", "Account");

            // Admin goes to admin dashboard
            if (HttpContext.Session.GetString("IsAdmin") == "true")
                return RedirectToAction("Dashboard", "Admin");

            // Patient goes to their home page
            return View();
        }
    }
}