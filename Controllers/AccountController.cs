using Microsoft.AspNetCore.Mvc;
using PTS_Apparel.Data;
using PTS_Apparel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace PTS_Apparel.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Login Screen
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login Action
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("FullName", user.FullName);
                return RedirectToAction("Dashboard", "Home");
            }
            ViewBag.Error = "Invalid Username or Password";
            return View();
        }
        //
        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        
        [HttpGet]
        public IActionResult GetUserRole(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Json(new { role = "", fullName = "" });
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                return Json(new { role = user.Role, fullName = user.FullName });
            }

            return Json(new { role = "", fullName = "" });
        }
    }
}