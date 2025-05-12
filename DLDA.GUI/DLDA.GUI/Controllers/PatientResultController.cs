using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    public class PatientResultController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
