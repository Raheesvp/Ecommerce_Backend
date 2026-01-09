using Microsoft.AspNetCore.Mvc;

namespace Project.WebAPI.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
