using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_BD.Database;
using Project_BD.Models;

namespace Project_BD.Controllers
{
    public class UsersController : Controller
    {
        private readonly E_db _context;

        public UsersController(E_db context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: Users/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Users/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Special check for user's requested admin credentials
            if (email == "admin@gmail.com" && password == "admin123")
            {
                HttpContext.Session.SetInt32("UserId", 0); // Special ID for hardcoded admin
                HttpContext.Session.SetString("UserName", "System Admin");
                HttpContext.Session.SetString("UserRole", "Admin");
                HttpContext.Session.SetString("UserEmail", "admin@gmail.com");
                return RedirectToAction("Dashboard", "Admin");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserName", user.Name ?? "User");
                HttpContext.Session.SetString("UserRole", user.Role ?? "Student");
                HttpContext.Session.SetString("UserEmail", user.Email ?? "");

                if (user.Role == "Admin") return RedirectToAction("Dashboard", "Admin");
                return RedirectToAction("Dashboard", "Student");
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        // GET: Users/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            // Set default values before validation
            user.Role = "Student";

            // Remove Role from ModelState because it's not provided by the form but is marked as Required in some contexts
            ModelState.Remove("Role");

            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ViewBag.Error = "Email already registered.";
                    return View("Login", user);
                }

                _context.Add(user);
                await _context.SaveChangesAsync();

                // Auto-login after registration
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserName", user.Name ?? "User");
                HttpContext.Session.SetString("UserRole", user.Role ?? "Student");
                HttpContext.Session.SetString("UserEmail", user.Email ?? "");

                return RedirectToAction("Dashboard", "Student");
            }

            // If we got here, something is wrong, stay on the register tab
            ViewBag.Error = "Please check your information and try again.";
            ViewBag.IsRegister = true;
            return View("Login", user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
