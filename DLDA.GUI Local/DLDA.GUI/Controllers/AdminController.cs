using Microsoft.AspNetCore.Mvc;
using DLDA.GUI.Authorization;

namespace DLDA.GUI.Controllers
{
    [RoleAuthorize("admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
