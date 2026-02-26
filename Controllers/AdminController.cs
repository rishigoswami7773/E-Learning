using Microsoft.AspNetCore.Mvc;

namespace Project_BD.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult ManageUsers()
        {
            return View();
        }

        public IActionResult Instructors()
        {
            return View();
        }

        public IActionResult Courses()
        {
            return View();
        }

        public IActionResult CourseApproval()
        {
            return View();
        }

        public IActionResult Payments()
        {
            return View();
        }

        public IActionResult Analytics()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }
    }
}
