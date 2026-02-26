using Microsoft.AspNetCore.Mvc;

namespace Project_BD.Controllers
{
    public class LessonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
