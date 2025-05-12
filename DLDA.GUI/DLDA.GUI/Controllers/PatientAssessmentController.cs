using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    public class PatientAssessmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
