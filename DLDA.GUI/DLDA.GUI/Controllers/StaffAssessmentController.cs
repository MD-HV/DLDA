using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace DLDA.GUI.Controllers
{
    [RoleAuthorize("staff")]
    public class StaffAssessmentController : Controller
    {
        private readonly HttpClient _httpClient;

        public StaffAssessmentController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DLDA");
        }

        // GET: /StaffAssessment
        // Visar indexvyn för personalens bedömningsmodul med alla patienter
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Hämta patienter med senaste bedömning från API:t
                var patients = await _httpClient.GetFromJsonAsync<List<PatientWithLatestAssessmentDto>>("User/with-latest-assessment");
                return View(patients ?? new List<PatientWithLatestAssessmentDto>());
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = $"Kunde inte hämta patienter: {ex.Message}";
                return View(new List<PatientWithLatestAssessmentDto>());
            }
        }

        // --------------------------
        // [PERSONAL] – Bedömningsöversikt för specifik patient
        // --------------------------

        // GET: /StaffAssessment/Assessments/{userId}
        // Hämtar alla bedömningar för en specifik patient
        public async Task<IActionResult> Assessments(int userId)
        {
            ViewBag.UserId = userId;

            var response = await _httpClient.GetAsync($"Assessment/user/{userId}");
            if (!response.IsSuccessStatusCode)
                return View("Error");

            var assessments = await response.Content.ReadFromJsonAsync<List<AssessmentDto>>();
            return View("Assessments", assessments ?? new List<AssessmentDto>());
        }

        // --------------------------
        // [PERSONAL] – Skapa ny bedömning
        // --------------------------

        // POST: /StaffAssessment/CreateForPatient
        // Skapar ny bedömning och redirectar tillbaka till Assessments
        [HttpPost]
        public async Task<IActionResult> CreateForPatient(int userId)
        {
            var dto = new AssessmentDto
            {
                UserId = userId,
                ScaleType = "Numerisk",
                IsComplete = false
            };

            var response = await _httpClient.PostAsJsonAsync("Assessment", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Misslyckades att skapa bedömning: {response.StatusCode} - {error}";
                return RedirectToAction("Assessments", new { userId });
            }

            TempData["Success"] = "Ny bedömning skapades.";
            return RedirectToAction("Assessments", new { userId });
        }

        // --------------------------
        // [PERSONAL] – Radera bedömning
        // --------------------------

        // GET: /StaffAssessment/Delete/{id}
        // Visar bekräftelsesidan
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"Assessment/{id}");
            if (!response.IsSuccessStatusCode)
                return View("Error");

            var assessment = await response.Content.ReadFromJsonAsync<AssessmentDto>();
            return View("Delete", assessment);
        }

        // POST: /StaffAssessment/DeleteConfirmed
        // Utför raderingen av en bedömning
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int userId)
        {
            var response = await _httpClient.DeleteAsync($"Assessment/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = $"Misslyckades att ta bort bedömning: {response.StatusCode}";
            }
            else
            {
                TempData["Success"] = "Bedömning togs bort.";
            }

            return RedirectToAction("Assessments", new { userId });
        }
    }
}
