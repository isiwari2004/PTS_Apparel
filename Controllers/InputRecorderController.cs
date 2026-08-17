using Microsoft.AspNetCore.Mvc;
using PTS_Apparel.Data;
using PTS_Apparel.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;
using System;

namespace PTS_Apparel.Controllers
{
    public class InputRecorderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InputRecorderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Input Recorder List (Tabs, Search, Date Filter)
        public IActionResult Index(string factoryFilter = "AVA", string searchTerm = "", string dateFilter = "")
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            // 1. Permission Check
            var currentUserRole = HttpContext.Session.GetString("Role");
            var perms = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Input Recorder" && p.Role == currentUserRole);
            ViewBag.CanAdd = perms != null && perms.CanAdd;
            ViewBag.CanEdit = perms != null && perms.CanEdit;

            var factories = _context.FactoryMasters
                .Select(f => f.FactoryCode)
                .Distinct()
                .OrderBy(f => f)
                .ToList();
            ViewBag.Factories = factories;
            
            ViewBag.CurrentFactory = string.IsNullOrEmpty(factoryFilter) ? factories.FirstOrDefault() : factoryFilter;
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentDate = dateFilter;

            // 3. Base Query
            var query = _context.InputRecorders.AsQueryable();

            
            var currentFactory = ViewBag.CurrentFactory as string;
            if (!string.IsNullOrEmpty(currentFactory))
            {
                query = query.Where(r => r.FactoryName == currentFactory);
            }

            // 5. Search Filter (Line, PO, Style, Colour)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r => r.LineNo.Contains(searchTerm) 
                                    || r.PONo.Contains(searchTerm) 
                                    || r.StyleNo.Contains(searchTerm) 
                                    || r.Colour.Contains(searchTerm));
            }

            // 6. Date Filter
            if (!string.IsNullOrWhiteSpace(dateFilter))
            {
                DateTime dt;
                if (DateTime.TryParse(dateFilter, out dt))
                {
                    query = query.Where(r => r.RecordDate.Date == dt.Date);
                }
            }

            // 7. Execute & Order
            var records = query.OrderBy(r => r.LineNo).ToList();
            return View(records);
        }

        // POST: Add / Edit Input Recorder (Upsert)
        [HttpPost]
        public IActionResult Upsert([FromForm] InputRecorder record)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired" });

            if (ModelState.IsValid)
            {
                if (record.Id == 0)
                    _context.InputRecorders.Add(record);
                else
                    _context.InputRecorders.Update(record);

                _context.SaveChanges();
                return Json(new { success = true, message = "Saved successfully!" });
            }
            
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = string.Join(", ", errors) });
        }

        // GET: Single Record for Edit (AJAX)
        [HttpGet]
        public IActionResult GetRecord(int id)
        {
            var record = _context.InputRecorders.Find(id);
            if (record == null) return Json(new { success = false });
            return Json(new { success = true, data = record });
        }

        // POST: Delete Record
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var record = _context.InputRecorders.Find(id);
            if (record == null) return Json(new { success = false });

            _context.InputRecorders.Remove(record);
            _context.SaveChanges();
            return Json(new { success = true });
        }
    }
}