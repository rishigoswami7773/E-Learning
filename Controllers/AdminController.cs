using System.Security.Cryptography;
using System.Text;
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
            // Remove optional fields from validation
            ModelState.Remove("Mobile");
            ModelState.Remove("Gender");
            ModelState.Remove("Address");
            ModelState.Remove("PhotoPath");

            if (ModelState.IsValid)
            {
                // Hash the password exactly like the login does
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(user.Password));
                user.Password = Convert.ToBase64String(hashedBytes);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                TempData["UserSuccess"] = $"✅ User '{user.Name}' ({user.Role}) created successfully!";
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

        public async Task<IActionResult> CourseApproval(string? filter = "pending")
        {
            ViewBag.Filter = filter ?? "pending";
            var query = _context.Courses.Include(c => c.Category).AsQueryable();
            if (filter == "approved")
                query = query.Where(c => c.IsApproved);
            else
                query = query.Where(c => !c.IsApproved);

            ViewBag.PendingCount = await _context.Courses.CountAsync(c => !c.IsApproved);
            ViewBag.ApprovedCount = await _context.Courses.CountAsync(c => c.IsApproved);

            var courses = await query.OrderByDescending(c => c.CourseId).ToListAsync();
            return View(courses);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCourse(int id, string? returnUrl = null)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                course.IsApproved = true;
                await _context.SaveChangesAsync();
                TempData["ApprovalSuccess"] = $"✅ Course '{course.Title}' has been approved successfully!";
            }
            else
            {
                TempData["ApprovalError"] = "Course not found.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(CourseApproval));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCourse(int id, string? returnUrl = null)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                string title = course.Title;
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                TempData["ApprovalSuccess"] = $"🗑️ Course '{title}' has been rejected and removed.";
            }
            else
            {
                TempData["ApprovalError"] = "Course not found.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(CourseApproval));
        }

        public async Task<IActionResult> Profile()
        {
            var user = await ResolveAdminUserForProfileAsync();
            if (user == null)
            {
                if (HttpContext.Session.GetString("UserRole") != "Admin")
                {
                    return RedirectToAction("Login", "Users");
                }

                var adminName = HttpContext.Session.GetString("UserName") ?? "System Admin";
                return View(new AdminProfileUpdateViewModel
                {
                    Name = adminName,
                    Email = HttpContext.Session.GetString("UserEmail") ?? "admin@gmail.com",
                    Role = "Admin",
                    Gender = "Other",
                    Mobile = string.Empty,
                    Address = string.Empty
                });
            }

            return View(new AdminProfileUpdateViewModel
            {
                Name = user.Name,
                Email = user.Email,
                Mobile = user.Mobile,
                Gender = user.Gender,
                Address = user.Address,
                Role = user.Role ?? "Admin",
                PhotoPath = user.PhotoPath
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(AdminProfileUpdateViewModel model, IFormFile? photo)
        {
            var user = await ResolveAdminUserForProfileAsync();
            if (user == null)
            {
                TempData["AdminProfileError"] = "Session expired. Please login again.";
                return RedirectToAction("Login", "Users");
            }

            model.Email = user.Email;
            model.Role = user.Role ?? "Admin";
            model.PhotoPath = user.PhotoPath;
            if (!ModelState.IsValid)
            {
                TempData["AdminProfileError"] = "Please fix validation errors and try again.";
                return View(model);
            }

            user.Name = model.Name?.Trim() ?? user.Name;
            user.Mobile = model.Mobile?.Trim() ?? user.Mobile;
            user.Gender = model.Gender?.Trim() ?? user.Gender;
            user.Address = model.Address?.Trim() ?? user.Address;

            if (photo != null && photo.Length > 0)
            {
                string imagesRoot = Path.Combine(_hostEnvironment.WebRootPath, "images", "users");
                if (!Directory.Exists(imagesRoot))
                {
                    Directory.CreateDirectory(imagesRoot);
                }

                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                string filePath = Path.Combine(imagesRoot, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                if (!string.IsNullOrWhiteSpace(user.PhotoPath))
                {
                    string oldFilePath = Path.Combine(_hostEnvironment.WebRootPath, user.PhotoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                user.PhotoPath = $"/images/users/{fileName}";
            }

            _context.Update(user);
            await _context.SaveChangesAsync();
            HttpContext.Session.SetString("UserName", user.Name);
            TempData["AdminProfileSuccess"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
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

        // ---------------------------------------------------------------
        // QUIZ MANAGEMENT
        // ---------------------------------------------------------------

        // GET: Admin/ManageQuizzes
        public async Task<IActionResult> ManageQuizzes()
        {
            var quizzes = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions)
                .OrderBy(q => q.Course!.Title)
                .ToListAsync();
            return View(quizzes);
        }

        // GET: Admin/ManageQuestions/5  (quizId)
        public async Task<IActionResult> ManageQuestions(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.Questions!)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null) return NotFound();
            return View(quiz);
        }

        // GET: Admin/AddQuestion/5  (quizId)
        public async Task<IActionResult> AddQuestion(int id)
        {
            var quiz = await _context.Quizzes.Include(q => q.Course).FirstOrDefaultAsync(q => q.QuizId == id);
            if (quiz == null) return NotFound();
            ViewBag.Quiz = quiz;
            return View();
        }

        // POST: Admin/AddQuestion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(int quizId, string questionText,
            string option1, bool correct1,
            string option2, bool correct2,
            string option3, bool correct3,
            string option4, bool correct4)
        {
            if (string.IsNullOrWhiteSpace(questionText))
            {
                TempData["QuizMsg"] = "Question text is required.";
                return RedirectToAction(nameof(AddQuestion), new { id = quizId });
            }

            var question = new Question { QuestionText = questionText.Trim(), QuizId = quizId };
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var options = new[]
            {
                new Option { OptionText = option1?.Trim() ?? "", IsCorrect = correct1, QuestionId = question.QuestionId },
                new Option { OptionText = option2?.Trim() ?? "", IsCorrect = correct2, QuestionId = question.QuestionId },
                new Option { OptionText = option3?.Trim() ?? "", IsCorrect = correct3, QuestionId = question.QuestionId },
                new Option { OptionText = option4?.Trim() ?? "", IsCorrect = correct4, QuestionId = question.QuestionId }
            };
            _context.Options.AddRange(options.Where(o => !string.IsNullOrWhiteSpace(o.OptionText)));
            await _context.SaveChangesAsync();

            TempData["QuizSuccess"] = "Question added successfully!";
            return RedirectToAction(nameof(ManageQuestions), new { id = quizId });
        }

        // GET: Admin/EditQuestion/5  (questionId)
        public async Task<IActionResult> EditQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .Include(q => q.Quiz)
                    .ThenInclude(q => q!.Course)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question == null) return NotFound();
            return View(question);
        }

        // POST: Admin/EditQuestion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuestion(int questionId, string questionText,
            int optionId1, string option1, bool correct1,
            int optionId2, string option2, bool correct2,
            int optionId3, string option3, bool correct3,
            int optionId4, string option4, bool correct4)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuestionId == questionId);

            if (question == null) return NotFound();

            question.QuestionText = questionText?.Trim() ?? question.QuestionText;

            var updates = new[]
            {
                (optionId1, option1, correct1),
                (optionId2, option2, correct2),
                (optionId3, option3, correct3),
                (optionId4, option4, correct4)
            };

            foreach (var (oid, oText, oCorrect) in updates)
            {
                var opt = question.Options?.FirstOrDefault(o => o.OptionId == oid);
                if (opt != null)
                {
                    opt.OptionText = oText?.Trim() ?? opt.OptionText;
                    opt.IsCorrect  = oCorrect;
                }
            }

            await _context.SaveChangesAsync();
            TempData["QuizSuccess"] = "Question updated successfully!";
            return RedirectToAction(nameof(ManageQuestions), new { id = question.QuizId });
        }

        // POST: Admin/DeleteQuestion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.QuestionId == id);

            if (question != null)
            {
                if (question.Options != null)
                    _context.Options.RemoveRange(question.Options);
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
                TempData["QuizSuccess"] = "Question deleted.";
            }

            int quizId = question?.QuizId ?? 0;
            return RedirectToAction(nameof(ManageQuestions), new { id = quizId });
        }

        public async Task<IActionResult> Notifications()
        {
            var messages = await _context.ContactMessages.OrderByDescending(m => m.SentAt).ToListAsync();
            return View(messages);
        }

        /// <summary>
        /// Resolves the logged-in admin user. Repairs sessions where UserId was 0 (old built-in admin login).
        /// </summary>
        private async Task<User?> ResolveAdminUserForProfileAsync()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return null;

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue && userId.Value > 0)
            {
                var byId = await _context.Users.FindAsync(userId.Value);
                if (byId != null)
                    return byId;
            }

            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var byEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (byEmail != null)
            {
                HttpContext.Session.SetInt32("UserId", byEmail.UserId);
                return byEmail;
            }

            if (email.Equals("admin@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                var created = await EnsureBuiltinAdminUserAsync();
                HttpContext.Session.SetInt32("UserId", created.UserId);
                return created;
            }

            return null;
        }

        private async Task<User> EnsureBuiltinAdminUserAsync()
        {
            const string adminEmail = "admin@gmail.com";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (user != null)
                return user;

            user = new User
            {
                Name = "System Admin",
                Email = adminEmail,
                Password = HashAdminPassword("admin123"),
                Role = "Admin",
                Mobile = "0000000000",
                Gender = "Other",
                Address = ""
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        private static string HashAdminPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
