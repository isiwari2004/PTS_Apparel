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

            var currentUserRole = HttpContext.Session.GetString("Role");

            var perms = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Factory" && p.Role == currentUserRole);

            ViewBag.CanViewFactory = perms != null && perms.CanView;
            ViewBag.CanAddFactory = perms != null && perms.CanAdd;
            ViewBag.CanEditFactory = perms != null && perms.CanEdit;
            ViewBag.CanDeleteFactory = perms != null && perms.CanDelete;

            // Join කිරීමෙන් Master සහ Details දත්ත එකට ගන්නවා
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

        // POST: Create New Factory (Add Master + Details + Production Lines)
        [HttpPost]
        public IActionResult Create([FromForm] FactoryMaster master, [FromForm] int WorkingHours, [FromForm] decimal CycleTime, [FromForm] int ProdLines, [FromForm] List<string> LineNames)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired. Please login again." });

            var currentUserRole = HttpContext.Session.GetString("Role");
            var addPerm = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Factory" && p.Role == currentUserRole);
            if (addPerm == null || !addPerm.CanAdd)
                return Json(new { success = false, message = "You do not have permission to add factories." });

            // FactoryCode Check කිරීම
            var existing = _context.FactoryMasters.FirstOrDefault(f => f.FactoryCode == master.FactoryCode);
            if (existing != null)
                return Json(new { success = false, message = "Factory Code already exists." });

            try
            {
                // 1. Save Master
                _context.FactoryMasters.Add(master);
                _context.SaveChanges(); // මෙතනින් master.Id එක generate වෙනවා

                // 2. Save Details (මෙතන master.Id එක පාවිච්චි කරනවා)
                var detail = new FactoryDetail
                {
                    FactoryMasterId = master.Id,
                    WorkingHours = WorkingHours,
                    CycleTime = CycleTime,
                    ProdLines = ProdLines
                };
                _context.FactoryDetails.Add(detail);
                _context.SaveChanges();

                // 🚀 3. Save Production Lines (User ටයිප් කරපු නම් ටික Save වෙයි)
                if (LineNames != null && LineNames.Count > 0)
                {
                    for (int i = 0; i < LineNames.Count; i++)
                    {
                        // User ලා හිස් අකුරු දැම්මොත් අපි Default එකක් දාමු
                        var nameToSave = string.IsNullOrWhiteSpace(LineNames[i]) ? "Line " + (i + 1) : LineNames[i].Trim();

                        var newLine = new FactoryProductionLine
                        {
                            FactoryMasterId = master.Id,
                            LineNumber = i + 1,
                            LineName = nameToSave
                        };
                        _context.FactoryProductionLines.Add(newLine);
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

        // GET: Edit Factory (AJAX) - Master සහ Details දෙකම ගන්නවා
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

        // GET: Get Existing Lines for Edit (AJAX) - මේක Edit කරනකොට හිටපු line names ටික ගන්න
        [HttpGet]
        public IActionResult GetLines(int factoryMasterId)
        {
            var lines = _context.FactoryProductionLines
                                .Where(l => l.FactoryMasterId == factoryMasterId)
                                .OrderBy(l => l.LineNumber)
                                .Select(l => l.LineName)
                                .ToList();
            return Json(lines);
        }

        // POST: Update Factory (Edit Master + Details)
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
                // 1. Find Master
                var master = _context.FactoryMasters.Find(Id);
                if (master == null) return Json(new { success = false, message = "Factory Master not found." });

                // Check Duplicate Code
                var existing = _context.FactoryMasters.FirstOrDefault(f => f.FactoryCode == FactoryCode && f.Id != Id);
                if (existing != null)
                    return Json(new { success = false, message = "Factory Code already exists." });

                // 2. Update Master
                master.FactoryCode = FactoryCode;
                master.FactoryName = FactoryName;
                _context.FactoryMasters.Update(master);

                // 3. Find and Update Details
                var detail = _context.FactoryDetails.FirstOrDefault(d => d.FactoryMasterId == Id);
                if (detail != null)
                {
                    detail.WorkingHours = WorkingHours;
                    detail.CycleTime = CycleTime;
                    detail.ProdLines = ProdLines;
                    _context.FactoryDetails.Update(detail);
                }

                // 🚀 4. Update Production Lines (Delete existing, then add new ones)
                // Edit කරනකොට හිටපු lines ටික මකලා අලුත් ඒවා දානවා
                var existingLines = _context.FactoryProductionLines.Where(l => l.FactoryMasterId == Id).ToList();
                _context.FactoryProductionLines.RemoveRange(existingLines);

                if (LineNames != null && LineNames.Count > 0)
                {
                    for (int i = 0; i < LineNames.Count; i++)
                    {
                        var nameToSave = string.IsNullOrWhiteSpace(LineNames[i]) ? "Line " + (i + 1) : LineNames[i].Trim();

                        var newLine = new FactoryProductionLine
                        {
                            FactoryMasterId = Id,
                            LineNumber = i + 1,
                            LineName = nameToSave
                        };
                        _context.FactoryProductionLines.Add(newLine);
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

        // POST: Delete Factory (Master + Details + Production Lines)
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
                var master = _context.FactoryMasters.Find(id);
                if (master == null)
                    return Json(new { success = false, message = "Factory not found." });

                // 🚀 පළමුව මේ Factory එකට අයිති Production Lines ටික මකන්න (අතින්)
                var lines = _context.FactoryProductionLines.Where(l => l.FactoryMasterId == id).ToList();
                if (lines.Any())
                {
                    _context.FactoryProductionLines.RemoveRange(lines);
                }

                // 🚀 ඊළඟට මේ Factory එකට අයිති Details එක මකන්න (අතින්)
                var details = _context.FactoryDetails.Where(d => d.FactoryMasterId == id).ToList();
                if (details.Any())
                {
                    _context.FactoryDetails.RemoveRange(details);
                }

                // 🚀 අන්තිමට Factory Master එක මකන්න
                _context.FactoryMasters.Remove(master);

                // සියල්ල එක වර Save කරන්න
                _context.SaveChanges();

                return Json(new { success = true, message = "Factory deleted successfully!" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // GET: Search Factories (AJAX)
        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // Search හිස් නම් ඔක්කොම ගන්නවා
                var all = (from m in _context.FactoryMasters
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
                return Json(all);
            }

            var results = (from m in _context.FactoryMasters
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
                           }).ToList();
            return Json(results);
        }
    }
}