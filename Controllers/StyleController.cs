using Microsoft.AspNetCore.Mvc;
using PTS_Apparel.Data;
using PTS_Apparel.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using ExcelDataReader;

namespace PTS_Apparel.Controllers
{
    public class StyleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StyleController(ApplicationDbContext context)
        {
            _context = context;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        // GET: Style List 
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            var currentUserRole = HttpContext.Session.GetString("Role");
            var perms = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Style" && p.Role == currentUserRole);
            ViewBag.CanAdd = perms != null && perms.CanAdd;
            ViewBag.CanEdit = perms != null && perms.CanEdit;
            ViewBag.CanDelete = perms != null && perms.CanDelete;

            ViewBag.Customers = _context.Customers.ToList();

            var styles = (from m in _context.StyleMasters
                          join d in _context.StyleDetails on m.Id equals d.StyleMasterId
                          orderby m.StyleCode
                          select new
                          {
                              Id = m.Id,
                              CustomerName = m.CustomerName,
                              StyleCode = m.StyleCode,
                              ColorCode = d.ColorCode,
                              Sizes = d.Sizes
                          }).ToList();

            return View(styles);
        }

        // GET: Create Style Page
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");
            
            ViewBag.Customers = _context.Customers.ToList();
            return View();
        }

        // POST: Save Style (Insert Master + Detail)
        [HttpPost]
        public IActionResult SaveStyle([FromBody] StyleSaveRequest request)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired" });

            try
            {
                if (string.IsNullOrEmpty(request.CustomerName) || string.IsNullOrEmpty(request.StyleCode) || string.IsNullOrEmpty(request.ColorCode) || string.IsNullOrEmpty(request.Sizes))
                    return Json(new { success = false, message = "Please fill all fields." });

                var existing = _context.StyleMasters.FirstOrDefault(s => s.StyleCode == request.StyleCode);
                if (existing != null)
                    return Json(new { success = false, message = "Style Code already exists." });

                var master = new StyleMaster
                {
                    CustomerName = request.CustomerName!,
                    StyleCode = request.StyleCode!,
                    CreatedAt = DateTime.Now
                };
                _context.StyleMasters.Add(master);
                _context.SaveChanges();

                var detail = new StyleDetail
                {
                    StyleMasterId = master.Id,
                    ColorCode = request.ColorCode!,
                    Sizes = request.Sizes!,
                    CreatedAt = DateTime.Now
                };
                _context.StyleDetails.Add(detail);
                _context.SaveChanges();

                return Json(new { success = true, message = "Style saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: Excel Upload කරලා Data Extract කරන Action එක (ඔබේ Excel එකට ගැලපෙන Columns)
        [HttpPost]
        public IActionResult UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "No file uploaded." });

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xls" && extension != ".xlsx")
                return Json(new { success = false, message = "Please upload a valid Excel file (.xlsx or .xls)." });

            try
            {
                List<string> sizesList = new List<string>();
                string extractedBuyer = "";
                string extractedStyle = "";
                string extractedColor = "";

                using (var stream = file.OpenReadStream())
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    int rowIndex = 0;
                    while (reader.Read())
                    {
                        // 👇 ඔබේ Excel Screenshot එකට අනුව Columns මෙතන වෙනස් කරලා තියෙනවා!
                        // Column B (Index 1) = Buyer
                        // Column C (Index 2) = Style
                        // Column F (Index 5) = Color
                        // Column O (Index 14) = Size

                        var colBuyer = reader.GetValue(1)?.ToString()?.Trim();  // Column B
                        var colStyle = reader.GetValue(2)?.ToString()?.Trim();  // Column C
                        var colColor = reader.GetValue(5)?.ToString()?.Trim();  // Column F
                        var colSize  = reader.GetValue(14)?.ToString()?.Trim(); // Column O

                        // පළමු දත්ත පේළියෙන් (Row 1) Master Data ගන්න (Row 0 Header එක හැර)
                        if (rowIndex == 1 && !string.IsNullOrEmpty(colBuyer))
                        {
                            extractedBuyer = colBuyer;
                            extractedStyle = colStyle ?? "";
                            extractedColor = colColor ?? "";
                        }

                        // සියලුම පේළි වලින් Size එකතු කරගන්න
                        if (rowIndex >= 1 && !string.IsNullOrEmpty(colSize))
                        {
                            sizesList.Add(colSize);
                        }

                        rowIndex++;
                    }
                }

                return Json(new 
                { 
                    success = true, 
                    buyer = extractedBuyer, 
                    style = extractedStyle, 
                    color = extractedColor, 
                    sizes = sizesList 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error reading file: " + ex.Message });
            }
        }

        // GET: Get Style for Edit (AJAX)
        [HttpGet]
        public IActionResult GetStyle(int id)
        {
            var styleData = (from m in _context.StyleMasters
                             join d in _context.StyleDetails on m.Id equals d.StyleMasterId
                             where m.Id == id
                             select new
                             {
                                 Id = m.Id,
                                 CustomerName = m.CustomerName,
                                 StyleCode = m.StyleCode,
                                 ColorCode = d.ColorCode,
                                 Sizes = d.Sizes
                             }).FirstOrDefault();

            if (styleData == null) return Json(new { success = false });
            return Json(new { success = true, data = styleData });
        }

        // POST: Delete Style
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var master = _context.StyleMasters.Find(id);
            if (master == null) return Json(new { success = false });

            _context.StyleMasters.Remove(master);
            _context.SaveChanges();
            return Json(new { success = true });
        }
    }

    public class StyleSaveRequest
    {
        public string? CustomerName { get; set; }
        public string? StyleCode { get; set; }
        public string? ColorCode { get; set; }
        public string? Sizes { get; set; }
    }
}