using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_BD.Database;
using Project_BD.Models;

namespace Project_BD.Controllers
{
    public class AdminController : Controller
    {
        private readonly E_db _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AdminController(E_db context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var courses = await _context.Courses.ToListAsync();
                bool hasUpdates = false;
                foreach (var course in courses.Where(c => string.IsNullOrEmpty(c.ThumbnailUrl)))
                {
                    string keyword = "education";
                    if (course.Title.Contains("React", StringComparison.OrdinalIgnoreCase) || course.Title.Contains("Web", StringComparison.OrdinalIgnoreCase)) keyword = "programing,web";
                    else if (course.Title.Contains("JavaScript", StringComparison.OrdinalIgnoreCase) || course.Title.Contains("JS", StringComparison.OrdinalIgnoreCase)) keyword = "javascript,code";
                    else if (course.Title.Contains("Python", StringComparison.OrdinalIgnoreCase)) keyword = "python,data";
                    else if (course.Title.Contains("Java", StringComparison.OrdinalIgnoreCase)) keyword = "java,technology";
                    
                    course.ThumbnailUrl = $"https://images.unsplash.com/photo-1517694712202-14dd9538aa97?q=80&w=800&auto=format&fit=crop&sig={course.CourseId}";
                    // Using dynamic high-quality Unsplash images for better visuals
                    if (keyword == "programing,web") course.ThumbnailUrl = "https://images.unsplash.com/photo-1498050108023-c5249f4df085?q=80&w=800&auto=format&fit=crop";
                    else if (keyword == "javascript,code") course.ThumbnailUrl = "https://images.unsplash.com/photo-1579468118864-1b9ea3c0db4a?q=80&w=800&auto=format&fit=crop";
                    else if (keyword == "python,data") course.ThumbnailUrl = "https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?q=80&w=800&auto=format&fit=crop";
                    else if (keyword == "java,technology") course.ThumbnailUrl = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97?q=80&w=800&auto=format&fit=crop";
                    
                    hasUpdates = true;
                }
                if (hasUpdates) await _context.SaveChangesAsync();

                ViewBag.TotalUsers = await _context.Users.CountAsync();
                ViewBag.TotalCourses = await _context.Courses.CountAsync();
                ViewBag.TotalEnrollments = await _context.Enrollments.CountAsync();

                var enrollments = await _context.Enrollments.Include(e => e.Course).ToListAsync();
                ViewBag.TotalRevenue = enrollments.Sum(e => e.Course?.Price ?? 0);

                ViewBag.RecentUsers = await _context.Users.OrderByDescending(u => u.UserId).Take(5).ToListAsync();
                ViewBag.PendingCourses = await _context.Courses.Where(c => !c.IsApproved).Include(c => c.Category).OrderByDescending(c => c.CourseId).Take(5).ToListAsync();
                ViewBag.UnreadContactCount = await _context.ContactMessages.CountAsync(m => !m.IsRead);
            }
            catch (Exception)
            {
                ViewBag.TotalUsers = 0;
                ViewBag.TotalCourses = 0;
                ViewBag.TotalEnrollments = 0;
                ViewBag.TotalRevenue = 0;
                ViewBag.RecentUsers = new List<User>();
                ViewBag.PendingCourses = new List<Course>();
                ViewBag.UnreadContactCount = 0;
            }
            return View();
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        // GET: Admin/CreateUser
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageUsers));
            }
            return View(user);
        }

        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, User user)
        {
            if (id != user.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Users.AnyAsync(u => u.UserId == user.UserId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(ManageUsers));
            }
            return View(user);
        }

        // POST: Admin/DeleteUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        public async Task<IActionResult> Instructors()
        {
            var instructors = await _context.Users.Where(u => u.Role == "Instructor").ToListAsync();
            return View(instructors);
        }

        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses.Include(c => c.Category).ToListAsync();
            return View(courses);
        }

        public async Task<IActionResult> CourseApproval()
        {
            var courses = await _context.Courses.Include(c => c.Category).Where(c => !c.IsApproved).ToListAsync();
            return View(courses);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                course.IsApproved = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(CourseApproval));
        }

        [HttpPost]
        public async Task<IActionResult> RejectCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course); // Rejecting removes it in this simple version
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(CourseApproval));
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            // If session is missing or strictly system admin
            if (!userId.HasValue || userId == 0)
            {
                var adminName = HttpContext.Session.GetString("UserName") ?? "System Admin";
                return View(new User { Name = adminName, Email = "admin@gmail.com", Role = "Admin" });
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Login", "Users");
            }
            return View(user);
        }

        public async Task<IActionResult> Payments()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();

            ViewBag.TotalRevenue = enrollments.Sum(e => e.Course?.Price ?? 0);
            return View(enrollments);
        }

        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category, IFormFile? image)
        {
            if (ModelState.IsValid)
            {
                if (image != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    string categoryPath = Path.Combine(wwwRootPath, @"images\categories");

                    if (!Directory.Exists(categoryPath))
                    {
                        Directory.CreateDirectory(categoryPath);
                    }

                    using (var fileStream = new FileStream(Path.Combine(categoryPath, fileName), FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }
                    category.ImageUrl = @"/images/categories/" + fileName;
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category, IFormFile? image)
        {
            if (id != category.CategoryId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    if (image != null)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        string categoryPath = Path.Combine(wwwRootPath, @"images\categories");

                        if (!Directory.Exists(categoryPath))
                        {
                            Directory.CreateDirectory(categoryPath);
                        }

                        if (!string.IsNullOrEmpty(category.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(wwwRootPath, category.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        using (var fileStream = new FileStream(Path.Combine(categoryPath, fileName), FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }
                        category.ImageUrl = @"/images/categories/" + fileName;
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Categories.AnyAsync(c => c.CategoryId == category.CategoryId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Categories));
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Categories));
        }

        public async Task<IActionResult> Analytics()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.ActiveLearners = await _context.Enrollments.Select(e => e.UserId).Distinct().CountAsync();

            var totalEnrollments = await _context.Enrollments.CountAsync();
            var totalQuizzes = await _context.Quizzes.CountAsync();
            var totalPassed = await _context.QuizAttempts.Where(a => a.IsPassed).CountAsync();

            ViewBag.CompletionRate = totalEnrollments > 0 ? (double)totalPassed / totalEnrollments * 100 : 0;

            // Enrollment by Category for chart
            ViewBag.CategoryStats = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c!.Category)
                .GroupBy(e => e.Course!.Category!.Name)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync();

            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }

        public async Task<IActionResult> Notifications()
        {
            var messages = await _context.ContactMessages.OrderByDescending(m => m.SentAt).ToListAsync();
            return View(messages);
        }
    }
}
