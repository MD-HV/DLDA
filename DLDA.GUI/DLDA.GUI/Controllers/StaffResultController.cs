using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;

// Visa resultat, Patient: visa egen förbättring (positiv feedback),
// Personal: visa förbättringar och försämringar per fråga eller kategori

namespace DLDA.GUI.Controllers
{
    [RoleAuthorize("staff")]
    public class StaffResultController : Controller
    {
        private readonly HttpClient _httpClient;

        public StaffResultController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DLDA");
        }

        // GET: /StaffResult/Index/{id}
        public async Task<IActionResult> Index(int id)
        {
            // 🔁 Ny endpoint för personalens sammanställning
            var response = await _httpClient.GetAsync($"AssessmentItem/staff/assessment/{id}/overview");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Kunde inte hämta personalsammanställning.";
                return RedirectToAction("Index", "StaffAssessment");
            }

            var overview = await response.Content.ReadFromJsonAsync<StaffResultOverviewDto>();
            if (overview == null)
            {
                TempData["Error"] = "Data saknas.";
                return RedirectToAction("Index", "StaffAssessment");
            }

            ViewBag.AssessmentId = id;
            return View("Index", overview);
        }
    }
}
