using Microsoft.AspNetCore.Mvc;

namespace Project_BD.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
