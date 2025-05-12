using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace DLDA.GUI.Controllers
{
    public class StaffAssessmentController : Controller
    {
        private readonly HttpClient _httpClient;

        public StaffAssessmentController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DLDA");
        }

        // --------------------------
        // [PERSONAL] – Full åtkomst till alla bedömningar
        // --------------------------

        // POST: /Assessment/CreateForPatient
        // Skapar ny bedömning och redirectar tillbaka till Assessments (endast för personal)
        [HttpPost]
        public async Task<IActionResult> CreateForPatient(int userId)
        {
            var dto = new AssessmentDto
            {
                UserId = userId,
                ScaleType = "Numerisk", // Standardval – patient kan ev. ändra senare
                IsComplete = false
            };

            var response = await _httpClient.PostAsJsonAsync("Assessment", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Misslyckades att skapa bedömning: {response.StatusCode} - {error}";
                return RedirectToAction("Assessments", "StaffQuiz", new { userId });
            }

            TempData["Success"] = "Ny bedömning skapades.";
            return RedirectToAction("Assessments", "StaffQuiz", new { userId });
        }

        // GET: /Assessment/Delete/{id}
        // Visar bekräftelsesidan
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.GetAsync($"Assessment/{id}");
            if (!response.IsSuccessStatusCode)
                return View("Error");

            var assessment = await response.Content.ReadFromJsonAsync<AssessmentDto>();
            return View(assessment);
        }

        // POST: /Assessment/DeleteConfirmed
        // Utför raderingen
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

            return RedirectToAction("Assessments", "StaffQuiz", new { userId });
        }


    }
}
