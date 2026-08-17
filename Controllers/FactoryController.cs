using Microsoft.AspNetCore.Mvc;
using PTS_Apparel.Data;
using PTS_Apparel.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json; 

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

            var currentUserRole = HttpContext.Session.GetString("Role");
            var perms = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Factory" && p.Role == currentUserRole);

            ViewBag.CanViewFactory = perms != null && perms.CanView;
            ViewBag.CanAddFactory = perms != null && perms.CanAdd;
            ViewBag.CanEditFactory = perms != null && perms.CanEdit;
            ViewBag.CanDeleteFactory = perms != null && perms.CanDelete;

            var factories = (from m in _context.FactoryMasters
                             join d in _context.FactoryDetails on m.Id equals d.FactoryMasterId
                             select new
                             {
                                 Id = m.Id,
                                 FactoryCode = m.FactoryCode,
                                 FactoryName = m.FactoryName,
                                 WorkingHours = d.WorkingHours,
                                 CycleTime = d.CycleTime,
                                 ProdLines = d.ProdLines
                             }).ToList();

            return View(factories);
        }

        // POST: Create New Factory (Add Master + Details + Factory Lines)
        [HttpPost]
        public IActionResult Create([FromForm] FactoryMaster master, [FromForm] int WorkingHours, [FromForm] decimal CycleTime, [FromForm] int ProdLines, [FromForm] List<string> LineNames)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired. Please login again." });

            var currentUserRole = HttpContext.Session.GetString("Role");
            var addPerm = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Factory" && p.Role == currentUserRole);
            if (addPerm == null || !addPerm.CanAdd)
                return Json(new { success = false, message = "You do not have permission to add factories." });

            var existing = _context.FactoryMasters.FirstOrDefault(f => f.FactoryCode == master.FactoryCode);
            if (existing != null)
                return Json(new { success = false, message = "Factory Code already exists." });

            try
            {
                // 1. Save Master
                master.CreatedAt = DateTime.Now;
                _context.FactoryMasters.Add(master);
                _context.SaveChanges();

                // 2. Save Details
                var detail = new FactoryDetail
                {
                    FactoryMasterId = master.Id,
                    WorkingHours = WorkingHours,
                    CycleTime = CycleTime,
                    ProdLines = ProdLines,
                    CreatedAt = DateTime.Now
                };
                _context.FactoryDetails.Add(detail);
                _context.SaveChanges();

                // 3. Save Factory Lines 
                if (LineNames != null && LineNames.Count > 0)
                {
                    for (int i = 0; i < LineNames.Count; i++)
                    {
                        var nameToSave = string.IsNullOrWhiteSpace(LineNames[i]) ? "Line " + (i + 1) : LineNames[i].Trim();

                        var newLine = new FactoryLine
                        {
                            FactoryMasterId = master.Id,
                            LineNumber = i + 1,
                            LineName = nameToSave,
                            CreatedAt = DateTime.Now
                        };
                        _context.FactoryLines.Add(newLine);
                    }
                    _context.SaveChanges();
                }

                return Json(new { success = true, message = "Factory added successfully!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // GET: Edit Factory (AJAX)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var factoryData = (from m in _context.FactoryMasters
                               join d in _context.FactoryDetails on m.Id equals d.FactoryMasterId
                               where m.Id == id
                               select new
                               {
                                   Id = m.Id,
                                   FactoryCode = m.FactoryCode,
                                   FactoryName = m.FactoryName,
                                   WorkingHours = d.WorkingHours,
                                   CycleTime = d.CycleTime,
                                   ProdLines = d.ProdLines
                               }).FirstOrDefault();

            if (factoryData == null)
                return Json(new { success = false, message = "Factory not found." });

            return Json(new { success = true, data = factoryData });
        }

        // GET: Get Existing Lines for Edit (AJAX) 
        [HttpGet]
        public IActionResult GetLines(int factoryMasterId)
        {
            var lines = _context.FactoryLines
                                .Where(l => l.FactoryMasterId == factoryMasterId)
                                .OrderBy(l => l.LineNumber)
                                .Select(l => l.LineName)
                                .ToList();
            return Json(lines);
        }

        // POST: Update Factory (Edit Master + Details + Lines)
        [HttpPost]
        public IActionResult Edit([FromForm] int Id, [FromForm] string FactoryCode, [FromForm] string FactoryName, [FromForm] int WorkingHours, [FromForm] decimal CycleTime, [FromForm] int ProdLines, [FromForm] List<string> LineNames)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired. Please login again." });

            var currentUserRole = HttpContext.Session.GetString("Role");
            var editPerm = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Factory" && p.Role == currentUserRole);
            if (editPerm == null || !editPerm.CanEdit)
                return Json(new { success = false, message = "You do not have permission to edit factories." });

            try
            {
                var master = _context.FactoryMasters.Find(Id);
                if (master == null) return Json(new { success = false, message = "Factory Master not found." });

                var existing = _context.FactoryMasters.FirstOrDefault(f => f.FactoryCode == FactoryCode && f.Id != Id);
                if (existing != null)
                    return Json(new { success = false, message = "Factory Code already exists." });

                master.FactoryCode = FactoryCode;
                master.FactoryName = FactoryName;
                _context.FactoryMasters.Update(master);

                var detail = _context.FactoryDetails.FirstOrDefault(d => d.FactoryMasterId == Id);
                if (detail != null)
                {
                    detail.WorkingHours = WorkingHours;
                    detail.CycleTime = CycleTime;
                    detail.ProdLines = ProdLines;
                    _context.FactoryDetails.Update(detail);
                }

                // 🚀 Update Factory Lines (Delete existing, then add new ones)
                var existingLines = _context.FactoryLines.Where(l => l.FactoryMasterId == Id).ToList();
                _context.FactoryLines.RemoveRange(existingLines);

                if (LineNames != null && LineNames.Count > 0)
                {
                    for (int i = 0; i < LineNames.Count; i++)
                    {
                        var nameToSave = string.IsNullOrWhiteSpace(LineNames[i]) ? "Line " + (i + 1) : LineNames[i].Trim();

                        var newLine = new FactoryLine
                        {
                            FactoryMasterId = Id,
                            LineNumber = i + 1,
                            LineName = nameToSave,
                            CreatedAt = DateTime.Now
                        };
                        _context.FactoryLines.Add(newLine);
                    }
                }
                _context.SaveChanges();

                return Json(new { success = true, message = "Factory updated successfully!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Delete Factory (Master + Details + Lines)
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired. Please login again." });

            var currentUserRole = HttpContext.Session.GetString("Role");
            var delPerm = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Factory" && p.Role == currentUserRole);
            if (delPerm == null || !delPerm.CanDelete)
                return Json(new { success = false, message = "You do not have permission to delete factories." });

            try
            {
                
                var lines = _context.FactoryLines.Where(l => l.FactoryMasterId == id);
                _context.FactoryLines.RemoveRange(lines);

                
                var details = _context.FactoryDetails.Where(d => d.FactoryMasterId == id);
                _context.FactoryDetails.RemoveRange(details);

                
                var master = _context.FactoryMasters.Find(id);
                if (master == null)
                    return Json(new { success = false, message = "Factory not found." });

                _context.FactoryMasters.Remove(master);
                _context.SaveChanges();
                return Json(new { success = true, message = "Factory deleted successfully!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // GET: Search Factories 
        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            List<dynamic> results;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                results = (from m in _context.FactoryMasters
                           join d in _context.FactoryDetails on m.Id equals d.FactoryMasterId
                           select new
                           {
                               Id = m.Id,
                               FactoryCode = m.FactoryCode,
                               FactoryName = m.FactoryName,
                               WorkingHours = d.WorkingHours,
                               CycleTime = d.CycleTime,
                               ProdLines = d.ProdLines
                           }).ToList<dynamic>();
            }
            else
            {
                results = (from m in _context.FactoryMasters
                           join d in _context.FactoryDetails on m.Id equals d.FactoryMasterId
                           where m.FactoryCode.Contains(searchTerm) || m.FactoryName.Contains(searchTerm)
                           select new
                           {
                               Id = m.Id,
                               FactoryCode = m.FactoryCode,
                               FactoryName = m.FactoryName,
                               WorkingHours = d.WorkingHours,
                               CycleTime = d.CycleTime,
                               ProdLines = d.ProdLines
                           }).ToList<dynamic>();
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null 
            };

            return Json(results, jsonOptions);
        }
    }
}