using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_BD.Database;
using Project_BD.Models;

namespace Project_BD.Controllers
{
    public class CourseController : Controller
    {
        private readonly E_db _context;

        public CourseController(E_db context)
        {
            _context = context;
        }

        // GET: Course
        public async Task<IActionResult> Index()
        {
            var courses = await _context.Courses.Include(c => c.Category).ToListAsync();
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
                .FirstOrDefaultAsync(m => m.CourseId == id);

            if (course == null) return NotFound();

            return View(course);
        }

        // GET: Course/Create
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // POST: Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
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
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.CourseId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
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
