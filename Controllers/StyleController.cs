using Microsoft.AspNetCore.Mvc;
using PTS_Apparel.Data;
using PTS_Apparel.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;

namespace PTS_Apparel.Controllers
{
    public class StyleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StyleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Style List (Table)
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var currentUserRole = HttpContext.Session.GetString("Role");
            var perms = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Style" && p.Role == currentUserRole);

            ViewBag.CanAdd = perms != null && perms.CanAdd;
            ViewBag.CanEdit = perms != null && perms.CanEdit;
            ViewBag.CanDelete = perms != null && perms.CanDelete;

            var styles = _context.Styles.ToList();
            return View(styles);
        }

        
        [HttpPost]
        public IActionResult SaveStyle([FromBody] Style style)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired" });

            if (string.IsNullOrEmpty(style.CustomerName) || string.IsNullOrEmpty(style.StyleCode) || string.IsNullOrEmpty(style.ColorCode) || string.IsNullOrEmpty(style.Sizes))
                return Json(new { success = false, message = "Please fill in all fields." });

            try
            {
                if (style.Id == 0) // Create New
                {
                    _context.Styles.Add(style);
                }
                else // Update Existing 
                {
                    _context.Styles.Update(style);
                }
                _context.SaveChanges();
                return Json(new { success = true, message = "Style saved successfully!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        
        [HttpGet]
        public IActionResult GetStyle(int id)
        {
            var style = _context.Styles.Find(id);
            if (style == null) return Json(new { success = false });
            return Json(new { success = true, data = style });
        }

        // POST: Delete
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var style = _context.Styles.Find(id);
            if (style == null) return Json(new { success = false });

            _context.Styles.Remove(style);
            _context.SaveChanges();
            return Json(new { success = true });
        }

        // GET: Search
        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Json(_context.Styles.ToList());

            var results = _context.Styles
                .Where(s => s.CustomerName.Contains(searchTerm) || s.StyleCode.Contains(searchTerm))
                .ToList();
            return Json(results);
        }
    }
}