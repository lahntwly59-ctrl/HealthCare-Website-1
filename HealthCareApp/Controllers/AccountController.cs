//using HealthCareApp.Models.Entities;
//using HealthCareApp.Data;
//using HealthCareApp.Models;
//using HealthCareApp.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System.Linq;

//namespace HealthCareApp.Controllers
//{
//    public class AccountController : Controller
//    {
//        private readonly AppDbContext _context;

//        public AccountController(AppDbContext context)
//        {
//            _context = context;
//        }

//        // GET: Account/Register
//        public IActionResult Register()
//        {
//            return View();
//        }

//        // POST: Account/Register
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Register(Patient patient)
//        {
//            if (ModelState.IsValid)
//            {
//                if (_context.Patients.Any(p => p.Email == patient.Email))
//                {
//                    ModelState.AddModelError("Email", "Email already exists.");
//                    return View(patient);
//                }

//                _context.Patients.Add(patient);
//                _context.SaveChanges();
//                return RedirectToAction("Login");
//            }
//            return View(patient);
//        }

//        // GET: Account/Login
//        public IActionResult Login()
//        {
//            return View();
//        }

//        // POST: Account/Login
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Login(string email, string password)
//        {
//            var user = _context.Patients.FirstOrDefault(p => p.Email == email && p.Password == password);
//            if (user != null)
//            {
//                HttpContext.Session.SetInt32("UserId", user.Patient_ID);
//                HttpContext.Session.SetString("UserName", user.FullName);
//                HttpContext.Session.SetString("UserRole", "Patient");
//                return RedirectToAction("Index", "Home");
//            }

//            // Simple Admin check (for demo purposes)
//            if (email == "admin@healthcare.com" && password == "admin123")
//            {
//                HttpContext.Session.SetInt32("UserId", 0);
//                HttpContext.Session.SetString("UserName", "Administrator");
//                HttpContext.Session.SetString("UserRole", "Admin");
//                return RedirectToAction("Dashboard", "Admin");
//            }

//            ViewBag.Error = "Invalid login attempt.";
//            return View();
//        }

//        // GET: Account/Logout
//        public IActionResult Logout()
//        {
//            HttpContext.Session.Clear();
//            return RedirectToAction("Login");
//        }

//        // GET: Account/Profile
//        public IActionResult Profile()
//        {
//            int? userId = HttpContext.Session.GetInt32("UserId");
//            if (userId == null || userId == 0) return RedirectToAction("Login");

//            var patient = _context.Patients.Find(userId);
//            return View(patient);
//        }

//        [HttpPost]
//        public IActionResult Profile(Patient updatedPatient)
//        {
//            var patient = _context.Patients.Find(updatedPatient.Patient_ID);
//            if (patient != null)
//            {
//                patient.FirstName = updatedPatient.FirstName;
//                patient.LastName = updatedPatient.LastName;
//                patient.Phone = updatedPatient.Phone;
//                patient.Address = updatedPatient.Address;
//                _context.SaveChanges();
//                HttpContext.Session.SetString("UserName", patient.FullName);
//                ViewBag.Message = "Profile updated successfully!";
//            }
//            return View(patient);
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthCareApp.Data;
using HealthCareApp.Models.Entities;
using HealthCareApp.Models.ViewModels;

namespace HealthCareApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        public AccountController(AppDbContext context) { _context = context; }

        // GET: /Account/Login
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("PatientId") != null)
                return RedirectToAction("Index", "Patient");
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Email == model.Email && p.Password == model.Password);

            if (patient == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32("PatientId", patient.Patient_ID);
            HttpContext.Session.SetString("PatientName", patient.FullName);
            return RedirectToAction("Index", "Patient");
        }

        // GET: /Account/Register
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            bool emailExists = await _context.Patients.AnyAsync(p => p.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            bool phoneExists = await _context.Patients.AnyAsync(p => p.Phone == model.Phone);
            if (phoneExists)
            {
                ModelState.AddModelError("Phone", "This phone number is already registered.");
                return View(model);
            }

            var patient = new Patient
            {
                FirstName     = model.FirstName,
                LastName      = model.LastName,
                Email         = model.Email,
                Password      = model.Password,
                Phone         = model.Phone,
                Address       = model.Address,
                Date_Of_Birth = model.Date_Of_Birth,
                Gender        = model.Gender
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("PatientId", patient.Patient_ID);
            HttpContext.Session.SetString("PatientName", patient.FullName);
            return RedirectToAction("Index", "Patient");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: /Account/ChangePassword
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetInt32("PatientId") == null)
                return RedirectToAction("Login");
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            int patientId = HttpContext.Session.GetInt32("PatientId") ?? 0;
            var patient = await _context.Patients.FindAsync(patientId);

            if (patient == null || patient.Password != model.OldPassword)
            {
                ModelState.AddModelError("OldPassword", "Current password is incorrect.");
                return View(model);
            }

            patient.Password = model.NewPassword;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction("Profile", "Patient");
        }
    }
}