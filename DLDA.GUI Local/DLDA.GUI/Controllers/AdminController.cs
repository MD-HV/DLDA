using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DLDA.GUI.Authorization;

namespace DLDA.GUI.Controllers
{
    [RoleAuthorize("admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Adminpanelen öppnades av användare {User}", HttpContext.Session.GetString("Username"));
            return View();
        }
    }
}
