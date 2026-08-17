using Microsoft.AspNetCore.Mvc;
using PTS_Apparel.Data;
using PTS_Apparel.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;

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

        // POST: Save PO 
        [HttpPost]
        public IActionResult SavePO([FromBody] AddPOViewModel model)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired" });

            if (model == null || string.IsNullOrEmpty(model.PONumber) || model.SizeDetails == null || model.SizeDetails.Count == 0)
                return Json(new { success = false, message = "Please fill in all fields correctly." });

            try
            {
              
                var newPO = new PO
                {
                    PONumber = model.PONumber,
                    Customer = "Import", 
                    Style = model.StyleCode,
                    Color = model.ColorCode,
                    Tolerance = model.Tolerance,
                    Quantity = model.SizeDetails.Sum(s => s.FinalQuantity), // Total Quantity ගණනය කිරීම
                    SizeBreakdownJson = JsonSerializer.Serialize(model.SizeDetails)
                };

                _context.POs.Add(newPO);
                _context.SaveChanges();
                
                return Json(new { success = true, message = "PO Saved Successfully!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // GET: Get Single PO for Edit (AJAX)
        [HttpGet]
        public IActionResult GetPO(int id)
        {
            var po = _context.POs.Find(id);
            if (po == null) return Json(new { success = false });
            return Json(new { success = true, data = po });
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