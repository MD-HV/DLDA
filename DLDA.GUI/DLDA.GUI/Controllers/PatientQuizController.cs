using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace DLDA.GUI.Controllers
{
    [RoleAuthorize("patient")]
    public class PatientQuizController : Controller
    {
        public IActionResult Info()
        {
            return View();
        }
    }
}
