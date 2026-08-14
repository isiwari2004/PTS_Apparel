using Microsoft.AspNetCore.Mvc;
using PTS_Apparel.Data;
using PTS_Apparel.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;
using System;

namespace PTS_Apparel.Controllers
{
    public class ForecastController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ForecastController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Forecast Index (Tabs, Search, Date Filter)
        public IActionResult Index(string factoryFilter = "Amava Apparel PVT Ltd", string searchTerm = "", string dateFilter = "")
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            // Permission Check
            var currentUserRole = HttpContext.Session.GetString("Role");
            var perms = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Forecast" && p.Role == currentUserRole);
            ViewBag.CanAdd = perms != null && perms.CanAdd;
            ViewBag.CanEdit = perms != null && perms.CanEdit;

            // 👇 ප්‍රධාන වෙනස: _context.Factories වෙනුවට _context.FactoryMasters
            // 1. Tabs සඳහා Factories ටික FactoryMasters Table එකෙන් ගන්නවා
            var factories = _context.FactoryMasters
                .Select(f => f.FactoryName)
                .Distinct()
                .OrderBy(f => f)
                .ToList();
            ViewBag.Factories = factories;
            ViewBag.CurrentFactory = factoryFilter;
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentDate = dateFilter;

            // 2. Base Query (ඔක්කොම දත්ත)
            var query = _context.Forecasts.AsQueryable();

            // 3. Factory Filter (Active Tab එකට අනුව)
            query = query.Where(f => f.FactoryName == factoryFilter);

            // 4. Search Filter (Style Code හෝ Line No වලින් හොයනවා)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(f => f.StyleCode.Contains(searchTerm) || f.LineNo.Contains(searchTerm));
            }

            // 5. Date Filter
            if (!string.IsNullOrWhiteSpace(dateFilter))
            {
                DateTime dt;
                if (DateTime.TryParse(dateFilter, out dt))
                {
                    query = query.Where(f => f.ForecastDate.Date == dt.Date);
                }
            }

            // 6. Execute Query
            var results = query.OrderBy(f => f.StyleCode).ToList();
            return View(results);
        }

        // POST: Add / Edit Forecast (AJAX)
        [HttpPost]
        public IActionResult Upsert(Forecast forecast)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired" });

            if (ModelState.IsValid)
            {
                if (forecast.Id == 0)
                    _context.Forecasts.Add(forecast);
                else
                    _context.Forecasts.Update(forecast);

                _context.SaveChanges();
                return Json(new { success = true, message = "Forecast saved successfully!" });
            }
            return Json(new { success = false, message = "Please fill all fields correctly." });
        }

        // GET: Get Single Forecast for Edit
        [HttpGet]
        public IActionResult GetForecast(int id)
        {
            var fc = _context.Forecasts.Find(id);
            if (fc == null) return Json(new { success = false });
            return Json(new { success = true, data = fc });
        }

        // POST: Delete Forecast
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var fc = _context.Forecasts.Find(id);
            if (fc == null) return Json(new { success = false });

            _context.Forecasts.Remove(fc);
            _context.SaveChanges();
            return Json(new { success = true });
        }
    }
}