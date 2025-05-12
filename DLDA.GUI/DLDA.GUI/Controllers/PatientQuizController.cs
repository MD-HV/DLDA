using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace DLDA.GUI.Controllers
{
    public class PatientQuizController : Controller
    {
        private readonly HttpClient _httpClient;

        public PatientQuizController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DLDA");
        }

        // GET: /PatientQuiz/Info
        // Visar informationssidan före quizstart
        public IActionResult Info()
        {
            return View();
        }

        // GET: /PatientQuiz
        // Visar patientens tidigare och pågående bedömningar
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var response = await _httpClient.GetAsync($"api/Assessment/user/{userId}");

            if (!response.IsSuccessStatusCode)
                return View(new List<AssessmentDto>()); // Visa tom lista vid fel

            var assessments = await response.Content.ReadFromJsonAsync<List<AssessmentDto>>();
            return View(assessments ?? new List<AssessmentDto>());
        }
    }
}
