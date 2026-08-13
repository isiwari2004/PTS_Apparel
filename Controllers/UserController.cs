using Microsoft.AspNetCore.Mvc;
using PTS_Apparel.Data;
using PTS_Apparel.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace PTS_Apparel.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public IActionResult Register()
        {
            
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        
        [HttpPost]
        public IActionResult Register(string username, string password, string role, string fullName)
        {
            
            if (HttpContext.Session.GetString("Username") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == username);
            if (existingUser != null)
            {
                ViewBag.Error = "This username already exists. Please choose another.";
                return View();
            }

            
            var newUser = new User
            {
                Username = username,
                Password = password,
                Role = role,
                FullName = fullName
            };

            
            _context.Users.Add(newUser);
            _context.SaveChanges();

            
            ViewBag.Success = "User registered successfully! You can now login.";
            return View();
        }
    }
}