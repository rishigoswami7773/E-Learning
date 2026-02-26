using Microsoft.AspNetCore.Mvc;

namespace Project_BD.Controllers
{
    public class QuizController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
