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

        // GET: Input Recorder Index
        public IActionResult Index(string factoryFilter = "AVA")
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            // Tabs සඳහා Factories ටික ගන්නවා
            var factories = _context.Factories.Select(f => f.FactoryCode).Distinct().ToList();
            ViewBag.Factories = factories;
            ViewBag.CurrentFactory = factoryFilter;

            // අදාල Factory එකේ දත්ත විතරක් ගන්නවා
            var records = _context.InputRecorders
                .Where(r => r.FactoryName == factoryFilter)
                .OrderBy(r => r.LineNo)
                .ToList();

            return View(records);
        }
    }
}