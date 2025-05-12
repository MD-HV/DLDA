using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace DLDA.GUI.Controllers
{
    [RoleAuthorize("staff")]
    public class StaffQuizController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
