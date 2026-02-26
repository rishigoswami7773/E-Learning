using Microsoft.AspNetCore.Mvc;

namespace Project_BD.Controllers
{
    public class ModuleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
