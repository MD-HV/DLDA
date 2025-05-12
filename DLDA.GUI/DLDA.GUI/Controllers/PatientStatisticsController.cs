using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    public class PatientStatisticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
