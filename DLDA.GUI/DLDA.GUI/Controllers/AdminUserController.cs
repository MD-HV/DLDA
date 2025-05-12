using DLDA.GUI.Authorization;
using Microsoft.AspNetCore.Mvc;

// Skapa/redigera användare (framför allt patienter)

namespace DLDA.GUI.Controllers
{
    [RoleAuthorize("admin")]
    public class AdminUserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
