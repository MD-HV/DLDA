using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace DLDA.API.Controllers
{
    public class StatisticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
