using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthCareApp.Data;
using HealthCareApp.Models.Entities;

namespace HealthCareApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        // Hard-coded original admin email
        private const string MasterAdmin = "lahntwly59@gmail.com";

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        private bool CheckIsAdmin(Patient p)
            => p.Email == MasterAdmin || p.Address == "ADMIN";

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("PatientId") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string Email, string Password)
        {
            var patient = _context.Patients
                .FirstOrDefault(p => p.Email == Email && p.Password == Password);

            if (patient == null)
            {
                TempData["Error"] = "Invalid email or password.";
                return RedirectToAction("Login");
            }

            HttpContext.Session.SetInt32("PatientId", patient.Patient_ID);
            HttpContext.Session.SetString("PatientName", patient.FullName);
            HttpContext.Session.SetString("IsAdmin", CheckIsAdmin(patient) ? "true" : "false");

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register  (patients only — admins are added by admin panel)
        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetInt32("PatientId") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string FirstName, string LastName, string Email,
                                       string Phone, string Gender, DateTime? Date_Of_Birth,
                                       string? Address, string Password, string ConfirmPassword)
        {
            if (Password != ConfirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return RedirectToAction("Register");
            }

            if (_context.Patients.Any(p => p.Email == Email))
            {
                TempData["Error"] = "This email is already registered.";
                return RedirectToAction("Register");
            }

            if (_context.Patients.Any(p => p.Phone == Phone))
            {
                TempData["Error"] = "This phone number is already registered.";
                return RedirectToAction("Register");
            }

            var patient = new Patient
            {
                FirstName     = FirstName,
                LastName      = LastName,
                Email         = Email,
                Password      = Password,
                Phone         = Phone,
                Gender        = Gender,
                Date_Of_Birth = Date_Of_Birth,
                Address       = Address
            };

            _context.Patients.Add(patient);
            _context.SaveChanges();

            HttpContext.Session.SetInt32("PatientId", patient.Patient_ID);
            HttpContext.Session.SetString("PatientName", patient.FullName);
            HttpContext.Session.SetString("IsAdmin", "false");

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: /Account/Profile
        [HttpGet]
        public IActionResult Profile()
        {
            int? id = HttpContext.Session.GetInt32("PatientId");
            if (id == null) return RedirectToAction("Login");
            var patient = _context.Patients.FirstOrDefault(p => p.Patient_ID == id);
            if (patient == null) return RedirectToAction("Login");
            return View(patient);
        }

        // GET: /Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (HttpContext.Session.GetInt32("PatientId") == null)
                return RedirectToAction("Login");
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string CurrentPassword,
                                             string NewPassword,
                                             string ConfirmNewPassword)
        {
            int? id = HttpContext.Session.GetInt32("PatientId");
            if (id == null) return RedirectToAction("Login");

            var patient = _context.Patients.FirstOrDefault(p => p.Patient_ID == id);
            if (patient == null) return RedirectToAction("Login");

            if (patient.Password != CurrentPassword)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("ChangePassword");
            }

            if (NewPassword != ConfirmNewPassword)
            {
                TempData["Error"] = "New passwords do not match.";
                return RedirectToAction("ChangePassword");
            }

            patient.Password = NewPassword;
            _context.SaveChanges();

            TempData["Success"] = "Password changed successfully.";
            return RedirectToAction("Profile");
        }
    }
}