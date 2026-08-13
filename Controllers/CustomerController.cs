using Microsoft.AspNetCore.Mvc;
using PTS_Apparel.Data;
using PTS_Apparel.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;

namespace PTS_Apparel.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer List (Index Page with Pagination)
        public IActionResult Index(int page = 1, string searchTerm = "")
        {
            if (HttpContext.Session.GetString("Username") == null)
                return RedirectToAction("Login", "Account");

            // 1. Permission Check
            var currentUserRole = HttpContext.Session.GetString("Role");
            var perms = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Customer" && p.Role == currentUserRole);

            ViewBag.CanAdd = perms != null && perms.CanAdd;
            ViewBag.CanEdit = perms != null && perms.CanEdit;
            ViewBag.CanDelete = perms != null && perms.CanDelete;

            // 2. Base Query
            var query = _context.Customers.AsQueryable();

            // 3. Apply Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.CustomerName.Contains(searchTerm) || c.CustomerType.Contains(searchTerm));
                ViewBag.CurrentSearch = searchTerm;
            }

            // 4. Pagination Settings
            int pageSize = 10; // එක පිටුවකට දත්ත 10ක්
            int totalRecords = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Page එක අංකය අනුව දත්ත ටික ගන්නවා
            var customers = query
                .OrderBy(c => c.CustomerName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 5. Pass Pagination Data to View using ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(customers);
        }

        // POST: Create New Customer
        [HttpPost]
        public IActionResult Create(Customer customer)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired. Please login again." });

            var currentUserRole = HttpContext.Session.GetString("Role");
            var addPerm = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Customer" && p.Role == currentUserRole);
            if (addPerm == null || !addPerm.CanAdd)
                return Json(new { success = false, message = "You do not have permission to add customers." });

            if (ModelState.IsValid)
            {
                _context.Customers.Add(customer);
                _context.SaveChanges();
                return Json(new { success = true, message = "Customer added successfully!" });
            }
            return Json(new { success = false, message = "Invalid data." });
        }

        // GET: Edit Customer (AJAX)
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer == null)
                return Json(new { success = false, message = "Customer not found." });
            return Json(new { success = true, data = customer });
        }

        // POST: Update Customer (Edit)
        [HttpPost]
        public IActionResult Edit(Customer customer)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired. Please login again." });

            var currentUserRole = HttpContext.Session.GetString("Role");
            var editPerm = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Customer" && p.Role == currentUserRole);
            if (editPerm == null || !editPerm.CanEdit)
                return Json(new { success = false, message = "You do not have permission to edit customers." });

            if (ModelState.IsValid)
            {
                _context.Customers.Update(customer);
                _context.SaveChanges();
                return Json(new { success = true, message = "Customer updated successfully!" });
            }
            return Json(new { success = false, message = "Invalid data." });
        }

        // POST: Delete Customer
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("Username") == null)
                return Json(new { success = false, message = "Session expired. Please login again." });

            var currentUserRole = HttpContext.Session.GetString("Role");
            var delPerm = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Customer" && p.Role == currentUserRole);
            if (delPerm == null || !delPerm.CanDelete)
                return Json(new { success = false, message = "You do not have permission to delete customers." });

            var customer = _context.Customers.Find(id);
            if (customer == null)
                return Json(new { success = false, message = "Customer not found." });

            _context.Customers.Remove(customer);
            _context.SaveChanges();
            return Json(new { success = true, message = "Customer deleted successfully!" });
        }

        // GET: Search Customers (AJAX) - Pagination නැතුව JSON ආකාරයෙන් Data ටික යවන්න
        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Json(_context.Customers.ToList());

            var results = _context.Customers
                .Where(c => c.CustomerName.Contains(searchTerm) || c.CustomerType.Contains(searchTerm))
                .ToList();
            return Json(results);
        }
    }
}