using Microsoft.AspNetCore.Mvc;
using PTS_Apparel.Data;
using PTS_Apparel.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;

namespace PTS_Apparel.Controllers
{
    public class POManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public POManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PO List (Index Page)
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            // Permission Check
            var currentUserRole = HttpContext.Session.GetString("Role");
            var perms = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "PO" && p.Role == currentUserRole);

            ViewBag.CanAdd = perms != null && perms.CanAdd;
            ViewBag.CanEdit = perms != null && perms.CanEdit;
            ViewBag.CanDelete = perms != null && perms.CanDelete;

            var poList = _context.POs.ToList();
            return View(poList);
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            PO model = new PO();
            if (id != null && id > 0)
            {
                model = _context.POs.Find(id);
                if (model == null) return RedirectToAction("Index");
            }
            return View("Create", model); 
        }

        // POST: Save PO (Add / Edit)
        [HttpPost]
        public IActionResult Upsert([FromForm] PO po)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired" });

            if (string.IsNullOrEmpty(po.PONumber) || string.IsNullOrEmpty(po.Customer) || string.IsNullOrEmpty(po.Style) || string.IsNullOrEmpty(po.Color) || po.Quantity <= 0)
                return Json(new { success = false, message = "Please fill in all fields correctly." });

            try
            {
                if (po.Id == 0) // Add New
                {
                    _context.POs.Add(po);
                }
                else // Edit Existing
                {
                    _context.POs.Update(po);
                }
                _context.SaveChanges();
                return Json(new { success = true, message = "PO saved successfully!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Delete
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var po = _context.POs.Find(id);
            if (po == null) return Json(new { success = false });

            _context.POs.Remove(po);
            _context.SaveChanges();
            return Json(new { success = true });
        }

        // GET: Search
        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Json(_context.POs.ToList());

            var results = _context.POs
                .Where(p => p.PONumber.Contains(searchTerm) || p.Customer.Contains(searchTerm) || p.Style.Contains(searchTerm) || p.Color.Contains(searchTerm))
                .ToList();
            return Json(results);
        }
    }
}