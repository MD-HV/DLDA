using DLDA.GUI.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    public class PatientResultController : Controller
    {
        [RoleAuthorize("patient")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
