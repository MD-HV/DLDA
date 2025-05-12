using Microsoft.AspNetCore.Mvc;

//  Övergripande statistik, Matchningsgrad, Diff-tabeller, Trendanalys över tid

namespace DLDA.GUI.Controllers
{
    public class StaffStatisticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
