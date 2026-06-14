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

        // GET: /Doctor/Index
        [HttpGet]
        public IActionResult Index(string? search, int? departmentId)
        {
            var doctors = _context.Doctors
                .Include(d => d.Department)
                .ToList();

            // Filter by name or field
            if (!string.IsNullOrEmpty(search))
                doctors = doctors.Where(d =>
                    d.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    d.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    d.Field.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            // Filter by department
            if (departmentId.HasValue && departmentId > 0)
                doctors = doctors.Where(d => d.Department_ID == departmentId).ToList();

            // Pass back to view so inputs keep their values
            ViewBag.Search       = search;
            ViewBag.DepartmentId = departmentId;
            ViewBag.Departments  = _context.Departments.ToList();

            return View(doctors);
        }

        // GET: /Doctor/Details/5
        [HttpGet]
        public IActionResult Details(int? id)
        {
            if (id == null) { TempData["Error"] = "No doctor selected."; return RedirectToAction("Index"); }

            var doctor = _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefault(d => d.Doctor_ID == id);

            if (doctor == null) { TempData["Error"] = "Doctor not found."; return RedirectToAction("Index"); }

            return View(doctor);
        }

        // GET: /Doctor/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Departments = _context.Departments.ToList();
            return View();
        }

        // POST: /Doctor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Doctor doctor)
        {
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

        // GET: /Doctor/Edit/5
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null) { TempData["Error"] = "No doctor selected."; return RedirectToAction("Index"); }

            var doctor = _context.Doctors.FirstOrDefault(d => d.Doctor_ID == id);

            if (doctor == null) { TempData["Error"] = "Doctor not found."; return RedirectToAction("Index"); }

            ViewBag.Departments = _context.Departments.ToList();
            return View(doctor);
        }

        // POST: /Doctor/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Doctor doctor)
        {
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

        // GET: /Doctor/Delete/5
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null) { TempData["Error"] = "No doctor selected."; return RedirectToAction("Index"); }

            var doctor = _context.Doctors
                .Include(d => d.Department)
                .FirstOrDefault(d => d.Doctor_ID == id);

            if (doctor == null) { TempData["Error"] = "Doctor not found."; return RedirectToAction("Index"); }

            return View(doctor);
        }

        // POST: /Doctor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var doctor = _context.Doctors.FirstOrDefault(d => d.Doctor_ID == id);

            if (doctor == null) { TempData["Error"] = "Doctor not found."; return RedirectToAction("Index"); }

            _context.Doctors.Remove(doctor);
            _context.SaveChanges();

            TempData["Success"] = "Doctor deleted successfully.";
            return RedirectToAction("Index");
        }
    }
}