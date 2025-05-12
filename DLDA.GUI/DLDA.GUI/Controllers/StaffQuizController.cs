using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace DLDA.GUI.Controllers
{
    public class StaffQuizController : Controller
    {
        private readonly HttpClient _httpClient;

        public StaffQuizController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DLDA");
        }

        // GET: /StaffQuiz/Index
        // Hämtar alla patienter med senaste bedömning
        public async Task<IActionResult> Index()
        {
            try
            {
                var patients = await _httpClient.GetFromJsonAsync<List<PatientWithLatestAssessmentDto>>("User/with-latest-assessment");
                return View(patients ?? new List<PatientWithLatestAssessmentDto>());
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Error = $"Kunde inte hämta patienter: {ex.Message}";
                return View(new List<PatientWithLatestAssessmentDto>());
            }
        }

        // GET: /StaffQuiz/Assessments/{userId}
        // Hämtar alla bedömningar för en specifik patient (både avslutade och pågående).
        public async Task<IActionResult> Assessments(int userId)
        {
            ViewBag.UserId = userId;

            var response = await _httpClient.GetAsync($"Assessment/user/{userId}");
            if (!response.IsSuccessStatusCode)
                return View("Error");

            var assessments = await response.Content.ReadFromJsonAsync<List<AssessmentDto>>();
            return View(assessments ?? new List<AssessmentDto>());
        }

    }
}
