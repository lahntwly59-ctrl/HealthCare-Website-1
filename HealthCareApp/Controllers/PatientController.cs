using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthCareApp.Data;
using HealthCareApp.Models.Entities;

namespace HealthCareApp.Controllers
{
    public class PatientController : Controller
    {
        private readonly AppDbContext _context;
        public PatientController(AppDbContext context) { _context = context; }

        private int? SessionPatientId => HttpContext.Session.GetInt32("PatientId");

        // GET: /Patient/Index  (Dashboard)
        public async Task<IActionResult> Index()
        {
            if (SessionPatientId == null) return RedirectToAction("Login", "Account");

            var patient = await _context.Patients.FindAsync(SessionPatientId);
            var appointmentsCount = await _context.Appointments
                .CountAsync(a => a.Patient_ID == SessionPatientId);
            var upcomingCount = await _context.Appointments
                .CountAsync(a => a.Patient_ID == SessionPatientId && a.Status == "Scheduled");

            ViewBag.AppointmentsCount = appointmentsCount;
            ViewBag.UpcomingCount = upcomingCount;
            return View(patient);
        }

        // GET: /Patient/Profile
        public async Task<IActionResult> Profile()
        {
            if (SessionPatientId == null) return RedirectToAction("Login", "Account");
            var patient = await _context.Patients.FindAsync(SessionPatientId);
            return View(patient);
        }

        // GET: /Patient/Edit
        public async Task<IActionResult> Edit()
        {
            if (SessionPatientId == null) return RedirectToAction("Login", "Account");
            var patient = await _context.Patients.FindAsync(SessionPatientId);
            return View(patient);
        }

        // POST: /Patient/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Patient model)
        {
            if (SessionPatientId == null) return RedirectToAction("Login", "Account");

            var patient = await _context.Patients.FindAsync(SessionPatientId);
            if (patient == null) return NotFound();

            patient.FirstName     = model.FirstName;
            patient.LastName      = model.LastName;
            patient.Phone         = model.Phone;
            patient.Address       = model.Address;
            patient.Date_Of_Birth = model.Date_Of_Birth;
            patient.Gender        = model.Gender;

            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("PatientName", patient.FullName);
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        // Admin: list all patients
        public async Task<IActionResult> All(string? search)
        {
            if (SessionPatientId == null) return RedirectToAction("Login", "Account");
            var query = _context.Patients.AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.FirstName.Contains(search) || p.LastName.Contains(search) || p.Email.Contains(search));
            ViewBag.Search = search;
            return View(await query.ToListAsync());
        }

        // GET: /Patient/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (SessionPatientId == null) return RedirectToAction("Login", "Account");
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        // POST: /Patient/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("All");
        }
    }
}