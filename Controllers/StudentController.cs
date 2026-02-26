using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_BD.Database;
using Project_BD.Models;

namespace Project_BD.Controllers
{
    public class StudentController : Controller
    {
        private readonly E_db _context;

        public StudentController(E_db context)
        {
            _context = context;
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

            return View();
        }
    }
}
