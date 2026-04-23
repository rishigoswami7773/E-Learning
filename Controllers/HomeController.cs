using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Project_BD.Models;
using Project_BD.Database;
using Microsoft.EntityFrameworkCore;

namespace Project_BD.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly E_db _context;

        public HomeController(ILogger<HomeController> logger, E_db context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.Take(4).ToListAsync();
            return View(categories);
        }

        public IActionResult ReactApp()
        {
            return PhysicalFile(
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/react/index.html"),
                "text/html"
            );
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // POST: Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMessage contactMessage)
        {
            if (ModelState.IsValid)
            {
                _context.ContactMessages.Add(contactMessage);
                await _context.SaveChangesAsync();
                ViewBag.Success = "Your message has been sent successfully! Our team will get back to you soon.";
                return View();
            }
            return View(contactMessage);
        }

        // Approve course using EF Core. Use POST from the UI.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCourse(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            if (!course.IsApproved)
            {
                course.IsApproved = true;
                await _context.SaveChangesAsync();
            }

            // Redirect to wherever you expect (home). If admin should go to admin approval page:
            // return RedirectToAction("CourseApproval", "Admin");
            return RedirectToAction(nameof(Index));
        }
    }
}
