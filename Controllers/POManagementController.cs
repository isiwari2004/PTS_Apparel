using Microsoft.AspNetCore.Mvc;
using PTS_Apparel.Data;
using PTS_Apparel.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json; // 👈 JSON සඳහා අවශ්‍යයි

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

            var currentUserRole = HttpContext.Session.GetString("Role");
            var perms = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "PO" && p.Role == currentUserRole);

            ViewBag.CanAdd = perms != null && perms.CanAdd;
            ViewBag.CanEdit = perms != null && perms.CanEdit;
            ViewBag.CanDelete = perms != null && perms.CanDelete;

            var poList = _context.POs.ToList();
            return View(poList);
        }

        // GET: Upsert (Add / Edit PO - Simple version for Edit)
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

        // POST: Save PO (Add / Edit - Simple)
        [HttpPost]
        public IActionResult Upsert([FromForm] PO po)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired" });

            if (string.IsNullOrEmpty(po.PONumber) || string.IsNullOrEmpty(po.Customer) || string.IsNullOrEmpty(po.Style) || string.IsNullOrEmpty(po.Color) || po.Quantity <= 0)
                return Json(new { success = false, message = "Please fill in all fields correctly." });

            try
            {
                if (po.Id == 0) _context.POs.Add(po);
                else _context.POs.Update(po);
                
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

        // ===========================================================
        // 👇 අලුත් Add PO (Size Breakdown & Tolerance) කොටස
        // ===========================================================

        // GET: Add PO Page (UI with Size Breakdown)
        [HttpGet]
        public IActionResult AddPO()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var styles = _context.Styles.Select(s => s.StyleCode).Distinct().ToList();
            ViewBag.Styles = styles;
            
            return View(new AddPOViewModel());
        }

        // AJAX: Style එකක් තෝරාගත් විට Colors ලබා ගැනීමට
        [HttpGet]
        public IActionResult GetColorsByStyle(string styleCode)
        {
            if (string.IsNullOrEmpty(styleCode)) return Json(new List<string>());

            var colors = _context.Styles
                .Where(s => s.StyleCode == styleCode)
                .Select(s => s.ColorCode)
                .Distinct()
                .ToList();

            return Json(colors);
        }

        // AJAX: Color එකක් තෝරාගත් විට Sizes ලබා ගැනීමට
        [HttpGet]
        public IActionResult GetSizesByStyleAndColor(string styleCode, string colorCode)
        {
            if (string.IsNullOrEmpty(styleCode) || string.IsNullOrEmpty(colorCode)) 
                return Json(new List<POSizeDetail>());

            var style = _context.Styles
                .FirstOrDefault(s => s.StyleCode == styleCode && s.ColorCode == colorCode);

            if (style == null) return Json(new List<POSizeDetail>());

            var sizes = style.Sizes.Split(',').Select(s => new POSizeDetail 
            { 
                SizeName = s, 
                OrderQty = 0, 
                ToleranceQty = 0, 
                FinalQuantity = 0 
            }).ToList();

            return Json(sizes);
        }

        // POST: Save PO (AddPO Form එකෙන් යවන දත්ත - Complex)
        [HttpPost]
        public IActionResult SavePO([FromBody] AddPOViewModel model)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired" });

            if (ModelState.IsValid)
            {
                // 1. අලුත් PO object එකක් හදාගන්නවා
                var newPO = new PO
                {
                    PONumber = model.PONumber,
                    Customer = "Imported", // මෙතනට ඔබට අවශ්‍ය Customer එක දාගන්න පුළුවන්
                    Style = model.StyleCode,
                    Color = model.ColorCode,
                    Tolerance = model.Tolerance,
                    Quantity = model.SizeDetails.Sum(s => s.FinalQuantity), // Total Quantity
                    SizeBreakdownJson = JsonSerializer.Serialize(model.SizeDetails)
                };

                _context.POs.Add(newPO);
                _context.SaveChanges();
                
                return Json(new { success = true, message = "PO Created Successfully!" });
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = string.Join(", ", errors) });
        }
    }
}