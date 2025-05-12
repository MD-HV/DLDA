using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace DLDA.GUI.Controllers
{
    [RoleAuthorize("patient")]
    public class PatientAssessmentController : Controller
    {
        private readonly HttpClient _httpClient;

        public PatientAssessmentController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DLDA");
        }

        // GET: /PatientAssessment/Info
        public IActionResult Info()
        {
            return View(); // Views/PatientAssessment/Info.cshtml
        }

        // GET: /PatientAssessment
        // Visar patientens egna bedömningar
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var response = await _httpClient.GetAsync($"Assessment/user/{userId}");

            if (!response.IsSuccessStatusCode)
                return View(new List<AssessmentDto>()); // Visa tom lista vid fel

            var assessments = await response.Content.ReadFromJsonAsync<List<AssessmentDto>>();
            return View(assessments ?? new List<AssessmentDto>());
        }
    }
}
