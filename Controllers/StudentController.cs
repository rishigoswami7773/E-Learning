using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_BD.Database;
using Project_BD.Models;

namespace Project_BD.Controllers
{
    public class StudentController : Controller
    {
        private readonly E_db _context;
        private readonly IWebHostEnvironment _environment;

        public StudentController(E_db context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Dashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Get user data with explicitly checked nullability for the collection
            var user = await _context.Users
                .Include(u => u.Enrollments!)
                    .ThenInclude(e => e.Course!)
                        .ThenInclude(c => c.Quizzes)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                // Fallback for system admin if path is hit
                if (userId == 0)
                {
                    ViewBag.UserName = "System Admin";
                    ViewBag.UserEmail = "admin@gmail.com";
                    ViewBag.EnrolledCourses = new List<Course>();
                    return View();
                }
                return RedirectToAction("Logout", "Users");
            }

            ViewBag.UserName = user.Name;
            ViewBag.UserEmail = user.Email;
            ViewBag.EnrolledCourses = user.Enrollments?
                .Where(e => e.Course != null)
                .Select(e => e.Course!)
                .ToList() ?? new List<Course>();

            // Fetch passed quizzes for certificates
            ViewBag.PassedQuizzes = await _context.QuizAttempts
                .Where(a => a.UserId == userId && a.IsPassed)
                .Select(a => a.QuizId)
                .ToListAsync();

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Users");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Logout", "Users");

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(User model, IFormFile? Photo)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Users");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Logout", "Users");

            // We are only updating a subset of properties
            ModelState.Remove("Password");
            ModelState.Remove("Email");
            ModelState.Remove("Role");
            ModelState.Remove("Enrollments");

            if (ModelState.IsValid)
            {
                user.Name = model.Name;
                user.Mobile = model.Mobile;
                user.Gender = model.Gender;
                user.Address = model.Address;

                if (Photo != null && Photo.Length > 0)
                {
                    var uploadsDir = Path.Combine(_environment.WebRootPath, "images", "users");
                    Directory.CreateDirectory(uploadsDir);
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Photo.FileName);
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Photo.CopyToAsync(stream);
                    }
                    user.PhotoPath = $"/images/users/{fileName}";
                }

                _context.Update(user);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("UserName", user.Name);

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Dashboard");
            }

            return View(user);
        }
    }
}
