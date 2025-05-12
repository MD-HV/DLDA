using Microsoft.AspNetCore.Mvc;

// Visa resultat, Patient: visa egen förbättring (positiv feedback),
// Personal: visa förbättringar och försämringar per fråga eller kategori

namespace DLDA.GUI.Controllers
{
    public class StaffResultController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
