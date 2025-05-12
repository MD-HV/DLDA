using Microsoft.AspNetCore.Mvc;

// Hantera frågedefinitioner: skapa, redigera, ta bort, lista

namespace DLDA.GUI.Controllers
{
    public class AdminQuestionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
