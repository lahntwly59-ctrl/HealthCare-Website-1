using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthCareApp.Data;
using HealthCareApp.Models.Entities;

namespace HealthCareApp.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Appointment/MyAppointments?status=Scheduled
        [HttpGet]
        public IActionResult MyAppointments(string? status)
        {
            int? patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null) return RedirectToAction("Login", "Account");

            var appointments = _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .Where(a => a.Patient_ID == patientId)
                .ToList();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status))
                appointments = appointments.Where(a => a.Status == status).ToList();

            ViewBag.Status = status; // keep active button highlighted
            return View(appointments.OrderByDescending(a => a.Appointment_Date).ToList());
        }

        // GET: /Appointment/Create?selectedDept=2
        [HttpGet]
        public IActionResult Create(int? selectedDept)
        {
            int? patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null) return RedirectToAction("Login", "Account");

            ViewBag.TodayMin     = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.Departments  = _context.Departments.ToList();
            ViewBag.SelectedDept = selectedDept;

            ViewBag.Doctors = selectedDept.HasValue
                ? _context.Doctors.Include(d => d.Department)
                    .Where(d => d.Department_ID == selectedDept).ToList()
                : _context.Doctors.Include(d => d.Department).ToList();

            return View();
        }

        // POST: /Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string AppointmentDateOnly,
                                    string AppointmentTimeSlot,
                                    int Doctor_ID,
                                    string? Description)
        {
            int? patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null) return RedirectToAction("Login", "Account");

            DateTime appointmentDate;
            bool parsed = DateTime.TryParse($"{AppointmentDateOnly} {AppointmentTimeSlot}", out appointmentDate);

            if (!parsed)
            {
                TempData["Error"] = "Invalid date or time selected.";
                return RedirectToAction("Create");
            }

            if (appointmentDate < DateTime.Now)
            {
                TempData["Error"] = "You cannot book an appointment in the past.";
                return RedirectToAction("Create");
            }

            var appointment = new Appointment
            {
                Patient_ID       = patientId.Value,
                Doctor_ID        = Doctor_ID,
                Appointment_Date = appointmentDate,
                Description      = Description,
                Status           = "Scheduled",
                CreatedAt        = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            TempData["Success"] = "Appointment booked successfully!";
            return RedirectToAction("MyAppointments");
        }

        // GET: /Appointment/Edit/5
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            int? patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null) return RedirectToAction("Login", "Account");

            if (id == null) { TempData["Error"] = "No appointment selected."; return RedirectToAction("MyAppointments"); }

            var appt = _context.Appointments
                .FirstOrDefault(a => a.Appointment_ID == id && a.Patient_ID == patientId);

            if (appt == null) { TempData["Error"] = "Appointment not found."; return RedirectToAction("MyAppointments"); }

            ViewBag.TodayMin     = DateTime.Today.ToString("yyyy-MM-dd");
            ViewBag.CurrentDate  = appt.Appointment_Date.ToString("yyyy-MM-dd");
            ViewBag.CurrentTime  = appt.Appointment_Date.ToString("HH:mm");
            ViewBag.Doctors      = _context.Doctors.Include(d => d.Department).ToList();
            ViewBag.Departments  = _context.Departments.ToList();

            return View(appt);
        }

        // POST: /Appointment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id,
                                   string AppointmentDateOnly,
                                   string AppointmentTimeSlot,
                                   int Doctor_ID,
                                   string? Description)
        {
            int? patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null) return RedirectToAction("Login", "Account");

            var appt = _context.Appointments
                .FirstOrDefault(a => a.Appointment_ID == id && a.Patient_ID == patientId);

            if (appt == null) { TempData["Error"] = "Appointment not found."; return RedirectToAction("MyAppointments"); }

            DateTime appointmentDate;
            bool parsed = DateTime.TryParse($"{AppointmentDateOnly} {AppointmentTimeSlot}", out appointmentDate);

            if (!parsed)
            {
                TempData["Error"] = "Invalid date or time.";
                return RedirectToAction("Edit", new { id });
            }

            if (appointmentDate < DateTime.Now)
            {
                TempData["Error"] = "You cannot set an appointment in the past.";
                return RedirectToAction("Edit", new { id });
            }

            appt.Doctor_ID        = Doctor_ID;
            appt.Appointment_Date = appointmentDate;
            appt.Description      = Description;

            _context.SaveChanges();
            TempData["Success"] = "Appointment updated successfully.";
            return RedirectToAction("MyAppointments");
        }

        // POST: /Appointment/ChangeStatus  ← NEW: lets patient mark as Completed or Canceled
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeStatus(int id, string newStatus)
        {
            int? patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null) return RedirectToAction("Login", "Account");

            var appt = _context.Appointments
                .FirstOrDefault(a => a.Appointment_ID == id && a.Patient_ID == patientId);

            if (appt == null) { TempData["Error"] = "Appointment not found."; return RedirectToAction("MyAppointments"); }

            // Only allow valid statuses
            var allowed = new[] { "Scheduled", "Completed", "Canceled" };
            if (!allowed.Contains(newStatus))
            {
                TempData["Error"] = "Invalid status.";
                return RedirectToAction("MyAppointments");
            }

            appt.Status = newStatus;
            _context.SaveChanges();

            TempData["Success"] = $"Appointment marked as {newStatus}.";
            return RedirectToAction("MyAppointments");
        }

        // GET: /Appointment/Delete/5
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            int? patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null) return RedirectToAction("Login", "Account");

            if (id == null) { TempData["Error"] = "No appointment selected."; return RedirectToAction("MyAppointments"); }

            var appt = _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.Appointment_ID == id && a.Patient_ID == patientId);

            if (appt == null) { TempData["Error"] = "Appointment not found."; return RedirectToAction("MyAppointments"); }

            return View(appt);
        }

        // POST: /Appointment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            int? patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null) return RedirectToAction("Login", "Account");

            var appt = _context.Appointments
                .FirstOrDefault(a => a.Appointment_ID == id && a.Patient_ID == patientId);

            if (appt == null) { TempData["Error"] = "Appointment not found."; return RedirectToAction("MyAppointments"); }

            _context.Appointments.Remove(appt);
            _context.SaveChanges();

            TempData["Success"] = "Appointment deleted.";
            return RedirectToAction("MyAppointments");
        }

        // GET: /Appointment/Details/5
        [HttpGet]
        public IActionResult Details(int? id)
        {
            int? patientId = HttpContext.Session.GetInt32("PatientId");
            if (patientId == null) return RedirectToAction("Login", "Account");

            if (id == null) { TempData["Error"] = "No appointment selected."; return RedirectToAction("MyAppointments"); }

            var appt = _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.Appointment_ID == id);

            if (appt == null) { TempData["Error"] = "Appointment not found."; return RedirectToAction("MyAppointments"); }

            return View(appt);
        }
    }
}