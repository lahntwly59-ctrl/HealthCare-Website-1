using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthCareApp.Data;
using HealthCareApp.Models.Entities;

namespace HealthCareApp.Controllers
{
    public class DoctorController : Controller
    {
        private readonly AppDbContext _context;

        public DoctorController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "true";
        private bool IsLoggedIn => HttpContext.Session.GetInt32("PatientId") != null;

        private IActionResult DenyAccess()
        {
            TempData["Error"] = "Access denied. Admins only.";
            return RedirectToAction("Index");
        }

        // GET: /Doctor/Index  — EVERYONE can view
        [HttpGet]
        public IActionResult Index(string? search, int? departmentId)
        {
            if (!IsLoggedIn) return RedirectToAction("Login", "Account");

            var doctors = _context.Doctors.Include(d => d.Department).ToList();

            if (!string.IsNullOrEmpty(search))
                doctors = doctors.Where(d =>
                    d.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    d.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    d.Field.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (departmentId.HasValue && departmentId > 0)
                doctors = doctors.Where(d => d.Department_ID == departmentId).ToList();

            ViewBag.Search       = search;
            ViewBag.DepartmentId = departmentId;
            ViewBag.Departments  = _context.Departments.ToList();

            return View(doctors);
        }

        // GET: /Doctor/Details/5  — EVERYONE can view
        [HttpGet]
        public IActionResult Details(int? id)
        {
            if (!IsLoggedIn) return RedirectToAction("Login", "Account");
            if (id == null) { TempData["Error"] = "No doctor selected."; return RedirectToAction("Index"); }

            var doctor = _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefault(d => d.Doctor_ID == id);

            if (doctor == null) { TempData["Error"] = "Doctor not found."; return RedirectToAction("Index"); }

            return View(doctor);
        }

        // GET: /Doctor/Create  — ADMIN ONLY
        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin) return DenyAccess();
            ViewBag.Departments = _context.Departments.ToList();
            return View();
        }

        // POST: /Doctor/Create  — ADMIN ONLY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Doctor doctor)
        {
            if (!IsAdmin) return DenyAccess();

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = _context.Departments.ToList();
                return View(doctor);
            }

            _context.Doctors.Add(doctor);
            _context.SaveChanges();
            TempData["Success"] = "Doctor added successfully.";
            return RedirectToAction("Index");
        }

        // GET: /Doctor/Edit/5  — ADMIN ONLY
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (!IsAdmin) return DenyAccess();
            if (id == null) { TempData["Error"] = "No doctor selected."; return RedirectToAction("Index"); }

            var doctor = _context.Doctors.FirstOrDefault(d => d.Doctor_ID == id);
            if (doctor == null) { TempData["Error"] = "Doctor not found."; return RedirectToAction("Index"); }

            ViewBag.Departments = _context.Departments.ToList();
            return View(doctor);
        }

        // POST: /Doctor/Edit  — ADMIN ONLY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Doctor doctor)
        {
            if (!IsAdmin) return DenyAccess();

            if (!ModelState.IsValid)
            {
                ViewBag.Departments = _context.Departments.ToList();
                return View(doctor);
            }

            var existing = _context.Doctors.FirstOrDefault(d => d.Doctor_ID == doctor.Doctor_ID);
            if (existing == null) { TempData["Error"] = "Doctor not found."; return RedirectToAction("Index"); }

            existing.FirstName     = doctor.FirstName;
            existing.LastName      = doctor.LastName;
            existing.Field         = doctor.Field;
            existing.Department_ID = doctor.Department_ID;
            existing.Phone         = doctor.Phone;
            existing.Email         = doctor.Email;

            _context.SaveChanges();
            TempData["Success"] = "Doctor updated successfully.";
            return RedirectToAction("Index");
        }

        // GET: /Doctor/Delete/5  — ADMIN ONLY
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (!IsAdmin) return DenyAccess();
            if (id == null) { TempData["Error"] = "No doctor selected."; return RedirectToAction("Index"); }

            var doctor = _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefault(d => d.Doctor_ID == id);

            if (doctor == null) { TempData["Error"] = "Doctor not found."; return RedirectToAction("Index"); }

            return View(doctor);
        }

        // POST: /Doctor/Delete/5  — ADMIN ONLY
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin) return DenyAccess();

            var doctor = _context.Doctors.FirstOrDefault(d => d.Doctor_ID == id);
            if (doctor == null) { TempData["Error"] = "Doctor not found."; return RedirectToAction("Index"); }

            _context.Doctors.Remove(doctor);
            _context.SaveChanges();
            TempData["Success"] = "Doctor deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}