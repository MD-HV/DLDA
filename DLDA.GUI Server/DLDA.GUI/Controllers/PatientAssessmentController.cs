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
        private readonly ILogger<PatientAssessmentController> _logger;

        public PatientAssessmentController(IHttpClientFactory httpClientFactory, ILogger<PatientAssessmentController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("DLDA");
            _logger = logger;
        }

        // GET: /PatientAssessment
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                _logger.LogWarning("Ingen inloggad användare – redirect till login.");
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var response = await _httpClient.GetAsync($"Assessment/user/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Misslyckades hämta bedömningar för patient ID {UserId}. Statuskod: {StatusCode}",
                        userId, response.StatusCode);

                    TempData["Error"] = "Kunde inte hämta dina bedömningar just nu.";
                    return View(new List<AssessmentDto>());
                }

                var assessments = await response.Content.ReadFromJsonAsync<List<AssessmentDto>>();
                return View(assessments ?? new List<AssessmentDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av bedömningar för patient ID {UserId}.", userId);
                TempData["Error"] = "Ett oväntat fel inträffade.";
                return View(new List<AssessmentDto>());
            }
        }
    }
}
