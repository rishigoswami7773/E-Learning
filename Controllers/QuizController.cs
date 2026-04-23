using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_BD.Database;
using Project_BD.Models;

namespace Project_BD.Controllers
{
    public class QuizController : Controller
    {
        private readonly E_db _context;

        public QuizController(E_db context)
        {
            _context = context;
        }

        // GET: Quiz/TakeQuiz/5
        public async Task<IActionResult> TakeQuiz(int? id)
        {
            if (id == null) return NotFound();

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions!)
                    .ThenInclude(qs => qs.Options)
                .FirstOrDefaultAsync(m => m.QuizId == id);

            if (quiz == null) return NotFound();

            // Check enrollment
            string? userRole = HttpContext.Session.GetString("UserRole");
            bool isEnrolled = false;

            if (userRole == "Admin")
            {
                isEnrolled = true;
            }
            else
            {
                isEnrolled = await _context.Enrollments.AnyAsync(e => e.CourseId == quiz.CourseId && e.UserId == userId);
            }

            if (!isEnrolled)
            {
                return RedirectToAction("Details", "Course", new { id = quiz.CourseId });
            }

            if (quiz.Questions == null || quiz.Questions.Count < 1)
            {
                TempData["QuizError"] = "This quiz has no questions yet. Please contact the administrator.";
                return RedirectToAction("Details", "Course", new { id = quiz.CourseId });
            }

            return View(quiz);
        }

        // POST: Quiz/SubmitQuiz
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(int quizId, IFormCollection form)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Users");

            var quiz = await _context.Quizzes
                .Include(q => q.Questions!)
                    .ThenInclude(qs => qs.Options)
                .FirstOrDefaultAsync(m => m.QuizId == quizId);

            if (quiz == null) return NotFound();

            if (quiz.Questions == null || quiz.Questions.Count < 1)
            {
                TempData["QuizError"] = "This quiz has no questions. Please contact the administrator.";
                return RedirectToAction("Details", "Course", new { id = quiz.CourseId });
            }

            var targetQuestions = quiz.Questions.ToList();
            int totalQuestions = targetQuestions.Count;
            int correctAnswers = 0;

            foreach (var question in targetQuestions)
            {
                string key = "question_" + question.QuestionId;
                if (form.ContainsKey(key))
                {
                    string? selectedValue = form[key];
                    if (int.TryParse(selectedValue, out int selectedOptionId))
                    {
                        var selectedOption = question.Options?.FirstOrDefault(o => o.OptionId == selectedOptionId);
                        if (selectedOption != null && selectedOption.IsCorrect)
                        {
                            correctAnswers++;
                        }
                    }
                }
            }

            double percentage = totalQuestions > 0 ? ((double)correctAnswers / totalQuestions) * 100 : 0;
            bool passed = percentage >= 70;

            // Save Attempt
            var attempt = new QuizAttempt
            {
                UserId = userId.Value,
                QuizId = quizId,
                Score = percentage,
                IsPassed = passed,
                CompletedAt = DateTime.Now
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            ViewBag.Correct = correctAnswers;
            ViewBag.Total = totalQuestions;
            ViewBag.Percentage = percentage;
            ViewBag.CourseTitle = (await _context.Courses.FindAsync(quiz.CourseId))?.Title;
            ViewBag.QuizId = quizId;

            return View("Result");
        }

        // GET: Quiz/Certificate/5
        public async Task<IActionResult> Certificate(int quizId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Users");

            var attempt = await _context.QuizAttempts
                .Where(a => a.QuizId == quizId && a.UserId == userId && a.IsPassed)
                .OrderByDescending(a => a.Score)
                .FirstOrDefaultAsync();

            if (attempt == null)
            {
                // Can't get a certificate if you haven't passed
                return RedirectToAction("TakeQuiz", new { id = quizId });
            }

            var quiz = await _context.Quizzes.Include(q => q.Course).FirstOrDefaultAsync(q => q.QuizId == quizId);
            if (quiz == null) return NotFound();

            ViewBag.CourseTitle = quiz.Course?.Title;
            ViewBag.Score = attempt.Score;
            ViewBag.Date = attempt.CompletedAt.ToString("MMMM dd, yyyy");
            ViewBag.StudentName = HttpContext.Session.GetString("UserName") ?? "Valued Student";

            return View();
        }
    }
}
