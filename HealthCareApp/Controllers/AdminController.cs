using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthCareApp.Data;
using HealthCareApp.Models.Entities;

namespace HealthCareApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "true";

        private IActionResult DenyAccess()
        {
            TempData["Error"] = "Access denied. Admins only.";
            return RedirectToAction("Index", "Home");
        }

        // ─── DASHBOARD ───────────────────────────────────────
        [HttpGet]
        public IActionResult Dashboard()
        {
            if (!IsAdmin) return DenyAccess();

            ViewBag.TotalPatients     = _context.Patients.Count();
            ViewBag.TotalDoctors      = _context.Doctors.Count();
            ViewBag.TotalDepartments  = _context.Departments.Count();
            ViewBag.TotalAppointments = _context.Appointments.Count();
            ViewBag.Scheduled         = _context.Appointments.Count(a => a.Status == "Scheduled");
            ViewBag.Completed         = _context.Appointments.Count(a => a.Status == "Completed");
            ViewBag.Canceled          = _context.Appointments.Count(a => a.Status == "Canceled");

            return View();
        }

        // ─── PATIENTS ────────────────────────────────────────
        [HttpGet]
        public IActionResult Patients(string? search)
        {
            if (!IsAdmin) return DenyAccess();

            var patients = _context.Patients.ToList();

            if (!string.IsNullOrEmpty(search))
                patients = patients.Where(p =>
                    p.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            ViewBag.Search = search;
            return View(patients);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePatient(int id)
        {
            if (!IsAdmin) return DenyAccess();

            var patient = _context.Patients.FirstOrDefault(p => p.Patient_ID == id);
            if (patient == null) { TempData["Error"] = "Patient not found."; return RedirectToAction("Patients"); }

            _context.Patients.Remove(patient);
            _context.SaveChanges();

            TempData["Success"] = "Patient deleted.";
            return RedirectToAction("Patients");
        }

        // ─── ALL APPOINTMENTS ─────────────────────────────────
        [HttpGet]
        public IActionResult Appointments(string? status)
        {
            if (!IsAdmin) return DenyAccess();

            var appointments = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .ToList();

            if (!string.IsNullOrEmpty(status))
                appointments = appointments.Where(a => a.Status == status).ToList();

            ViewBag.Status = status;
            return View(appointments.OrderByDescending(a => a.Appointment_Date).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeAppointmentStatus(int id, string newStatus)
        {
            if (!IsAdmin) return DenyAccess();

            var appt = _context.Appointments.FirstOrDefault(a => a.Appointment_ID == id);
            if (appt == null) { TempData["Error"] = "Appointment not found."; return RedirectToAction("Appointments"); }

            appt.Status = newStatus;
            _context.SaveChanges();

            TempData["Success"] = $"Appointment marked as {newStatus}.";
            return RedirectToAction("Appointments");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAppointment(int id)
        {
            if (!IsAdmin) return DenyAccess();

            var appt = _context.Appointments.FirstOrDefault(a => a.Appointment_ID == id);
            if (appt == null) { TempData["Error"] = "Appointment not found."; return RedirectToAction("Appointments"); }

            _context.Appointments.Remove(appt);
            _context.SaveChanges();

            TempData["Success"] = "Appointment deleted.";
            return RedirectToAction("Appointments");
        }

        // Admin books appointment for any patient
        [HttpGet]
        public IActionResult BookAppointment()
        {
            if (!IsAdmin) return DenyAccess();

            ViewBag.TodayMin    = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.Patients    = _context.Patients.ToList();
            ViewBag.Doctors     = _context.Doctors.Include(d => d.Department).ToList();
            ViewBag.Departments = _context.Departments.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BookAppointment(int Patient_ID, int Doctor_ID,
                                              string AppointmentDateOnly,
                                              string AppointmentTimeSlot,
                                              string? Description)
        {
            if (!IsAdmin) return DenyAccess();

            DateTime appointmentDate;
            bool parsed = DateTime.TryParse($"{AppointmentDateOnly} {AppointmentTimeSlot}", out appointmentDate);

            if (!parsed)
            {
                TempData["Error"] = "Invalid date or time.";
                return RedirectToAction("BookAppointment");
            }

            if (appointmentDate < DateTime.Now)
            {
                TempData["Error"] = "Cannot book an appointment in the past.";
                return RedirectToAction("BookAppointment");
            }

            var appt = new Appointment
            {
                Patient_ID       = Patient_ID,
                Doctor_ID        = Doctor_ID,
                Appointment_Date = appointmentDate,
                Description      = Description,
                Status           = "Scheduled",
                CreatedAt        = DateTime.Now
            };

            _context.Appointments.Add(appt);
            _context.SaveChanges();

            TempData["Success"] = "Appointment booked successfully.";
            return RedirectToAction("Appointments");
        }

        // ─── ADD ADMIN ────────────────────────────────────────
        [HttpGet]
        public IActionResult AddAdmin()
        {
            if (!IsAdmin) return DenyAccess();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAdmin(string FirstName, string LastName,
                                       string Email, string Phone,
                                       string Password, string ConfirmPassword)
        {
            if (!IsAdmin) return DenyAccess();

            if (Password != ConfirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return RedirectToAction("AddAdmin");
            }

            if (_context.Patients.Any(p => p.Email == Email))
            {
                TempData["Error"] = "This email is already registered.";
                return RedirectToAction("AddAdmin");
            }

            // Read current admin emails from a simple DB flag or just register as patient
            // We mark admins by storing their email in appsettings or by a Role field.
            // Simple approach: add to Patients with a known admin marker in Address field.
            var admin = new Patient
            {
                FirstName = FirstName,
                LastName  = LastName,
                Email     = Email,
                Phone     = Phone,
                Password  = Password,
                Gender    = "M",
                Address   = "ADMIN" // ← marker so we know this is an admin
            };

            _context.Patients.Add(admin);
            _context.SaveChanges();

            TempData["Success"] = $"{Email} has been added as an admin.";
            return RedirectToAction("Dashboard");
        }
    }
}