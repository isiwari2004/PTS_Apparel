using Microsoft.AspNetCore.Mvc;
using PTS_Apparel.Data;
using PTS_Apparel.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;

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

            var currentUserRole = HttpContext.Session.GetString("Role");
            var perms = _context.ScreenPrivileges.FirstOrDefault(p => p.ModuleName == "Customer" && p.Role == currentUserRole);

            ViewBag.CanAdd = perms != null && perms.CanAdd;
            ViewBag.CanEdit = perms != null && perms.CanEdit;
            ViewBag.CanDelete = perms != null && perms.CanDelete;

            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.CustomerName.Contains(searchTerm) || c.CustomerType.Contains(searchTerm));
                ViewBag.CurrentSearch = searchTerm;
            }

            int pageSize = 10;
            int totalRecords = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var customers = query
                .OrderBy(c => c.CustomerName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(customers);
        }

        // POST: Create New Customer
        [HttpPost]
        public IActionResult Create([FromForm] Customer customer)
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
        public IActionResult Edit([FromForm] Customer customer)
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

        // GET: Search Customers (AJAX)
        [HttpGet]
        public IActionResult Search(string searchTerm)
        {
            List<dynamic> results;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                results = _context.Customers
                    .Select(c => new
                    {
                        Id = c.Id,
                        CustomerName = c.CustomerName,
                        CustomerType = c.CustomerType
                    })
                    .ToList<dynamic>();
            }
            else
            {
                results = _context.Customers
                    .Where(c => c.CustomerName.Contains(searchTerm) || c.CustomerType.Contains(searchTerm))
                    .Select(c => new
                    {
                        Id = c.Id,
                        CustomerName = c.CustomerName,
                        CustomerType = c.CustomerType
                    })
                    .ToList<dynamic>();
            }

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            };

            return Json(results, jsonOptions);
        }

        // GET: Get All Customers (for Dropdown)
        [HttpGet]
        public IActionResult GetAllCustomers()
        {
            var customers = _context.Customers
                .Select(c => new
                {
                    Id = c.Id,
                    CustomerName = c.CustomerName,
                    CustomerType = c.CustomerType
                })
                .OrderBy(c => c.CustomerName)
                .ToList();

            return Json(customers);
        }
    }
}