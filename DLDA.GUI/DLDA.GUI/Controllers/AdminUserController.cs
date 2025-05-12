using Microsoft.AspNetCore.Mvc;

// Skapa/redigera användare (framför allt patienter)

namespace DLDA.GUI.Controllers
{
    public class AdminUserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
