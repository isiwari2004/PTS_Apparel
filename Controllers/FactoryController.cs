using Microsoft.AspNetCore.Mvc;
using PTS_Apparel.Data;
using PTS_Apparel.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;

namespace PTS_Apparel.Controllers
{
    public class FactoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FactoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Factory List (Index Page)
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var factories = _context.Factories.ToList();
            var currentUserRole = HttpContext.Session.GetString("Role");

            var perms = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Factory" && p.Role == currentUserRole);

            ViewBag.CanViewFactory = perms != null && perms.CanView;
            ViewBag.CanAddFactory = perms != null && perms.CanAdd;
            ViewBag.CanEditFactory = perms != null && perms.CanEdit;
            ViewBag.CanDeleteFactory = perms != null && perms.CanDelete;

            return View(factories);
        }

        // POST: Create New Factory (Add)
        [HttpPost]
        public IActionResult Create(Factory factory)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired. Please login again." });

            // Backend Add Check
            var currentUserRole = HttpContext.Session.GetString("Role");
            var addPerm = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Factory" && p.Role == currentUserRole);
            if (addPerm == null || !addPerm.CanAdd)
                return Json(new { success = false, message = "You do not have permission to add factories." });

            if (ModelState.IsValid)
            {
                var existing = _context.Factories.FirstOrDefault(f => f.FactoryCode == factory.FactoryCode);
                if (existing != null)
                    return Json(new { success = false, message = "Factory Code already exists." });

                _context.Factories.Add(factory);
                _context.SaveChanges();
                return Json(new { success = true, message = "Factory added successfully!" });
            }
            return Json(new { success = false, message = "Invalid data." });
        }

        // GET: Edit Factory (AJAX)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var factory = _context.Factories.Find(id);
            if (factory == null)
                return Json(new { success = false, message = "Factory not found." });
            return Json(new { success = true, data = factory });
        }

        // POST: Update Factory (Edit)
        [HttpPost]
        public IActionResult Edit(Factory factory)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired. Please login again." });

            var currentUserRole = HttpContext.Session.GetString("Role");
            var editPerm = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Factory" && p.Role == currentUserRole);
            if (editPerm == null || !editPerm.CanEdit)
                return Json(new { success = false, message = "You do not have permission to edit factories." });

            if (ModelState.IsValid)
            {
                var existing = _context.Factories.FirstOrDefault(f => f.FactoryCode == factory.FactoryCode && f.Id != factory.Id);
                if (existing != null)
                    return Json(new { success = false, message = "Factory Code already exists." });

                _context.Factories.Update(factory);
                _context.SaveChanges();
                return Json(new { success = true, message = "Factory updated successfully!" });
            }
            return Json(new { success = false, message = "Invalid data." });
        }

        // POST: Delete Factory
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired. Please login again." });

            var currentUserRole = HttpContext.Session.GetString("Role");
            var delPerm = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Factory" && p.Role == currentUserRole);
            if (delPerm == null || !delPerm.CanDelete)
                return Json(new { success = false, message = "You do not have permission to delete factories." });

            var factory = _context.Factories.Find(id);
            if (factory == null)
                return Json(new { success = false, message = "Factory not found." });

            _context.Factories.Remove(factory);
            _context.SaveChanges();
            return Json(new { success = true, message = "Factory deleted successfully!" });
        }

        // GET: Search Factories (AJAX)
        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Json(_context.Factories.ToList());

            var results = _context.Factories
                .Where(f => f.FactoryCode.Contains(searchTerm) || f.FactoryName.Contains(searchTerm))
                .ToList();
            return Json(results);
        }
    }
}