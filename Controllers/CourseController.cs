using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_BD.Database;
using Project_BD.Models;

namespace Project_BD.Controllers
{
    public class CourseController : Controller
    {
        private readonly E_db _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CourseController(E_db context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Course
        public async Task<IActionResult> Index(int? categoryId)
        {
            string? userRole = HttpContext.Session.GetString("UserRole");
            var query = _context.Courses.Include(c => c.Category).AsQueryable();

            if (userRole != "Admin")
            {
                query = query.Where(c => c.IsApproved);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
                ViewBag.SelectedCategory = (await _context.Categories.FindAsync(categoryId.Value))?.Name;
            }

            var courses = await query.ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(courses);
        }

        // GET: Course/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
                .Include(c => c.Quizzes)
                .FirstOrDefaultAsync(m => m.CourseId == id);

            if (course == null) return NotFound();

            // Check enrollment status
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? userRole = HttpContext.Session.GetString("UserRole");

            bool isEnrolled = false;
            if (userId != null)
            {
                // Admin bypasses enrollment check
                if (userRole == "Admin")
                {
                    isEnrolled = true;
                }
                else
                {
                    isEnrolled = await _context.Enrollments.AnyAsync(e => e.CourseId == id && e.UserId == userId);
                }
            }
            ViewBag.IsEnrolled = isEnrolled;
            ViewBag.UserRole = userRole;

            return View(course);
        }

        // POST: Course/Enroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var isAlreadyEnrolled = await _context.Enrollments.AnyAsync(e => e.CourseId == courseId && e.UserId == userId);
            if (!isAlreadyEnrolled)
            {
                var enrollment = new Enrollment
                {
                    CourseId = courseId,
                    UserId = userId.Value,
                    EnrollmentDate = DateTime.Now
                };
                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = courseId });
        }

        // GET: Course/WatchLesson/5
        public async Task<IActionResult> WatchLesson(int? id)
        {
            if (id == null) return NotFound();

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var lesson = await _context.Lessons
                .Include(l => l.Module)
                    .ThenInclude(m => m.Course)
                .FirstOrDefaultAsync(m => m.LessonId == id);

            if (lesson == null || lesson.Module == null) return NotFound();

            // Check if user is enrolled in the course this lesson belongs to
            int courseId = lesson.Module.CourseId;
            string? userRole = HttpContext.Session.GetString("UserRole");

            bool isEnrolled = false;
            if (userRole == "Admin")
            {
                isEnrolled = true;
            }
            else
            {
                isEnrolled = await _context.Enrollments.AnyAsync(e => e.CourseId == courseId && e.UserId == userId);
            }

            if (!isEnrolled)
            {
                return RedirectToAction(nameof(Details), new { id = courseId });
            }

            return View(lesson);
        }

        // GET: Course/Create
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course, IFormFile? thumbnail)
        {
            string? userRole = HttpContext.Session.GetString("UserRole");

            // Auto-approve if created by admin
            if (userRole == "Admin")
            {
                course.IsApproved = true;
            }

            if (ModelState.IsValid)
            {
                if (thumbnail != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnail.FileName);
                    string coursePath = Path.Combine(wwwRootPath, @"images\courses");

                    if (!Directory.Exists(coursePath))
                    {
                        Directory.CreateDirectory(coursePath);
                    }

                    using (var fileStream = new FileStream(Path.Combine(coursePath, fileName), FileMode.Create))
                    {
                        await thumbnail.CopyToAsync(fileStream);
                    }
                    course.ThumbnailUrl = @"/images/courses/" + fileName;
                }

                _context.Add(course);
                await _context.SaveChangesAsync();

                if (userRole == "Admin")
                {
                    return RedirectToAction("Courses", "Admin");
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(course);
        }

        // GET: Course/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            return View(course);
        }

        // POST: Course/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course, IFormFile? thumbnail)
        {
            if (id != course.CourseId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (thumbnail != null)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnail.FileName);
                        string coursePath = Path.Combine(wwwRootPath, @"images\courses");

                        if (!Directory.Exists(coursePath))
                        {
                            Directory.CreateDirectory(coursePath);
                        }

                        if (!string.IsNullOrEmpty(course.ThumbnailUrl))
                        {
                            var oldImagePath = Path.Combine(wwwRootPath, course.ThumbnailUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        using (var fileStream = new FileStream(Path.Combine(coursePath, fileName), FileMode.Create))
                        {
                            await thumbnail.CopyToAsync(fileStream);
                        }
                        course.ThumbnailUrl = @"/images/courses/" + fileName;
                    }

                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = _context.Categories.ToList();
            return View(course);
        }

        // GET: Course/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var course = await _context.Courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(m => m.CourseId == id);
            if (course == null) return NotFound();

            return View(course);
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                if (!string.IsNullOrEmpty(course.ThumbnailUrl))
                {
                    var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, course.ThumbnailUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                _context.Courses.Remove(course);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseId == id);
        }
    }
}
