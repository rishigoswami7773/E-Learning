using Microsoft.AspNetCore.Mvc;

namespace Project_BD.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
