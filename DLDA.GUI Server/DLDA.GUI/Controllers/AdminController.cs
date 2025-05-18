using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DLDA.GUI.Authorization;

namespace DLDA.GUI.Controllers
{
    /// <summary>
    /// Controller för adminpanelen. Endast åtkomlig för användare med rollen "admin".
    /// </summary>
    [RoleAuthorize("admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;

        /// <summary>
        /// Skapar en ny instans av AdminController.
        /// </summary>
        /// <param name="logger">Logger för loggning av aktiviteter i adminpanelen.</param>
        public AdminController(ILogger<AdminController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Visar adminpanelens startsida.
        /// </summary>
        /// <returns>Adminpanelens vy.</returns>
        public IActionResult Index()
        {
            _logger.LogInformation("Adminpanelen öppnades av användare {User}", HttpContext.Session.GetString("Username"));
            return View();
        }
    }
}
