using Microsoft.AspNetCore.Mvc;

namespace Project_BD.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
