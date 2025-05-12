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

        // Visar introduktionssidan med information om DLDA
        // GET: /PatientAssessment/Info
        public IActionResult Info()
        {
            return View(); // Views/PatientAssessment/Info.cshtml
        }

        // Visar patientens tidigare och pågående bedömningar
        // GET: /PatientAssessment/Index
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var response = await _httpClient.GetAsync($"api/Assessment/user/{userId}");
            if (!response.IsSuccessStatusCode)
                return View(new List<AssessmentDto>());

            var assessments = await response.Content.ReadFromJsonAsync<List<AssessmentDto>>();
            return View(assessments ?? new List<AssessmentDto>());
        }

        // GET: /StaffAssessment/Resume/{id}
        // Återupptar en pågående bedömning
        [HttpGet]
        public async Task<IActionResult> Resume(int id)
        {
            var response = await _httpClient.GetAsync($"Assessment/{id}");
            if (!response.IsSuccessStatusCode)
                return View("Error");

            var assessment = await response.Content.ReadFromJsonAsync<AssessmentDto>();
            return View("Resume", assessment); // Skapa Resume.cshtml om du inte har den än
        }
    }
}
