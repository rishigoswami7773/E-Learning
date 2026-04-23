using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_BD.Database;
using Project_BD.Models;

namespace Project_BD.Controllers
{
    public class ReviewController : Controller
    {
        private readonly E_db _context;

        public ReviewController(E_db context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrUpdate(int courseId, int rating, string comment)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Users");

            bool isEnrolled = await _context.Enrollments.AnyAsync(e => e.CourseId == courseId && e.UserId == userId);
            if (!isEnrolled)
            {
                TempData["ReviewError"] = "You can review this course only after enrollment.";
                return RedirectToAction("Details", "Course", new { id = courseId });
            }

            rating = Math.Clamp(rating, 1, 5);
            comment = (comment ?? string.Empty).Trim();
            if (comment.Length == 0)
            {
                TempData["ReviewError"] = "Please write a short review comment.";
                return RedirectToAction("Details", "Course", new { id = courseId });
            }

            var existing = await _context.Reviews.FirstOrDefaultAsync(r => r.CourseId == courseId && r.UserId == userId);
            if (existing == null)
            {
                _context.Reviews.Add(new Review
                {
                    CourseId = courseId,
                    UserId = userId.Value,
                    Rating = rating,
                    Comment = comment
                });
            }
            else
            {
                existing.Rating = rating;
                existing.Comment = comment;
            }

            await _context.SaveChangesAsync();
            TempData["ReviewSuccess"] = "Your review has been saved.";
            return RedirectToAction("Details", "Course", new { id = courseId });
        }
    }
}
