using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System;
using Project_BD.Database;
using Project_BD.Models;

namespace Project_BD.Controllers
{
    public class UsersController : Controller
    {
        private readonly E_db _context;
        private readonly IWebHostEnvironment _environment;

        public UsersController(E_db context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
            // Built-in admin: must map to a real User row so profile/save works (UserId 0 was breaking saves)
            if (email == "admin@gmail.com" && password == "admin123")
            {
                var adminUser = await GetOrCreateBuiltinAdminAsync();
                HttpContext.Session.SetInt32("UserId", adminUser.UserId);
                HttpContext.Session.SetString("UserName", adminUser.Name ?? "System Admin");
                HttpContext.Session.SetString("UserRole", "Admin");
                HttpContext.Session.SetString("UserEmail", adminUser.Email ?? "admin@gmail.com");
                return RedirectToAction("Dashboard", "Admin");
            }

            string hashedPassword = HashPassword(password);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == hashedPassword);

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
            return RedirectToAction("Login");
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ViewBag.Error = "Email already registered.";
                    ViewBag.IsRegister = true;
                    return View("Login", model);
                }

                // Create user entity
                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Password = HashPassword(model.Password),  // Hash password
                    Mobile = model.Mobile,
                    Gender = model.Gender,
                    Address = model.Address,
                    Role = "Student"
                };

                // Handle photo upload
                if (model.Photo != null && model.Photo.Length > 0)
                {
                    var uploadsDir = Path.Combine(_environment.WebRootPath, "images", "users");
                    Directory.CreateDirectory(uploadsDir); // Ensure directory exists
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Photo.FileName);
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Photo.CopyToAsync(stream);
                    }
                    user.PhotoPath = $"/images/users/{fileName}";
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Auto-login
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("UserEmail", user.Email);

                return RedirectToAction("Dashboard", "Student");
            }

            ViewBag.Error = "Please check your information and try again.";
            ViewBag.IsRegister = true;
            return View("Login", model);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>Ensures the default admin@gmail.com account exists in the database for session-backed features.</summary>
        private async Task<User> GetOrCreateBuiltinAdminAsync()
        {
            const string adminEmail = "admin@gmail.com";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (user != null)
            {
                return user;
            }

            user = new User
            {
                Name = "System Admin",
                Email = adminEmail,
                Password = HashPassword("admin123"),
                Role = "Admin",
                Mobile = "0000000000",
                Gender = "Other",
                Address = ""
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}

