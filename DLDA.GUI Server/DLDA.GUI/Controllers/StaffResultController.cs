using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

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
            try
            {
                var response = await _httpClient.GetAsync($"AssessmentItem/staff/assessment/{id}/overview");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Kunde inte hämta personalsammanställning.";
                    return RedirectToAction("Index", "StaffAssessment");
                }

                var overview = await response.Content.ReadFromJsonAsync<StaffResultOverviewDto>();
                if (overview == null)
                {
                    TempData["Error"] = "Data saknas för sammanställningen.";
                    return RedirectToAction("Index", "StaffAssessment");
                }

                return View("Index", overview);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
                return RedirectToAction("Index", "StaffAssessment");
            }
        }

        // POST: /StaffResult/UpdateStaffAnswer
        [HttpPost]
        public async Task<IActionResult> UpdateStaffAnswer(int itemId, int assessmentId, int answer, string? comment, bool flag)
        {
            try
            {
                var dto = new SubmitStaffAnswerDto
                {
                    ItemID = itemId,
                    Answer = answer,
                    Comment = comment,
                    Flag = flag
                };

                var response = await _httpClient.PostAsJsonAsync("Question/quiz/staff/submit", dto);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Kunde inte spara ändringar.";
                }
                else
                {
                    TempData["Success"] = "Svar uppdaterat.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
            }

            return RedirectToAction("Index", new { id = assessmentId });
        }

        // POST: /StaffResult/Complete
        [HttpPost]
        public async Task<IActionResult> Complete(int assessmentId, int userId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"AssessmentItem/assessment/{assessmentId}/staff-complete", null);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Kunde inte markera bedömningen som klar. Kontrollera att alla frågor är besvarade.";
                    return RedirectToAction("Index", new { id = assessmentId });
                }

                TempData["Success"] = "Personalens bedömning har markerats som klar.";
                return RedirectToAction("Assessments", "StaffAssessment", new { userId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
                return RedirectToAction("Index", new { id = assessmentId });
            }
        }

        // POST: /StaffAssessment/Unlock
        // låser upp en redan avklarad bedömning
        [HttpPost]
        public async Task<IActionResult> Unlock(int assessmentId, int userId)
        {
            var response = await _httpClient.PostAsync(
                $"assessment/unlock/{assessmentId}",
                null
            );

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Misslyckades med att låsa upp bedömningen.";
                return RedirectToAction("Assessments", new { userId });
            }

            TempData["Success"] = "Bedömningen har låsts upp.";
            return RedirectToAction("Index", new { id = assessmentId });
        }
    }
}
