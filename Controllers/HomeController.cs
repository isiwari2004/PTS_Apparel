using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using PTS_Apparel.Models;
using PTS_Apparel.Data;

namespace PTS_Apparel.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Dashboard
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            return View();
        }

        // 2. Screen Privileges Page (Load Data from DB)
        public IActionResult ScreenPrivileges()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var dbPermissions = _context.ScreenPrivileges.ToList();
            var modules = new List<string> { "Factory", "Customer", "Style", "PO", "Input Recorder", "Forecast", "Line Recorder", "Hourly Tracking", "Defect Report" };

            var model = new List<dynamic>();

            foreach (var mod in modules)
            {
                var admin = dbPermissions.FirstOrDefault(p => p.ModuleName == mod && p.Role == "Admin");
                var super = dbPermissions.FirstOrDefault(p => p.ModuleName == mod && p.Role == "Super User");
                var user = dbPermissions.FirstOrDefault(p => p.ModuleName == mod && p.Role == "User");

                model.Add(new
                {
                    ModuleName = mod,
                    Admin = new { V = admin?.CanView ?? false, A = admin?.CanAdd ?? false, E = admin?.CanEdit ?? false, D = admin?.CanDelete ?? false },
                    SuperUser = new { V = super?.CanView ?? false, A = super?.CanAdd ?? false, E = super?.CanEdit ?? false, D = super?.CanDelete ?? false },
                    User = new { V = user?.CanView ?? false, A = user?.CanAdd ?? false, E = user?.CanEdit ?? false, D = user?.CanDelete ?? false }
                });
            }
            return View(model);
        }

        // 3. SAVE CHANGES (AJAX Post)
        [HttpPost]
        public IActionResult SavePermissions([FromBody] List<ScreenPrivilege> permissions)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired" });

            try
            {
                
                var existing = _context.ScreenPrivileges.ToList();
                _context.ScreenPrivileges.RemoveRange(existing);
                _context.ScreenPrivileges.AddRange(permissions);
                _context.SaveChanges();

                return Json(new { success = true, message = "Permissions saved successfully!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Server Error: " + ex.Message });
            }
        }

        public IActionResult Index() { return RedirectToAction("Login", "Account"); }
        public IActionResult Privacy() { return View(); }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() { return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }); }
    }
}