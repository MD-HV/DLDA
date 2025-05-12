using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
