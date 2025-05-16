using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

[Route("StaffStatistics")]
[RoleAuthorize("staff")]
public class StaffStatisticsController : Controller
{
    private readonly HttpClient _httpClient;

    public StaffStatisticsController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("DLDA");
    }

    // GET: /StaffStatistics/Comparison/{assessmentId}
    [HttpGet("Comparison/{assessmentId}")]
    public async Task<IActionResult> Comparison(int assessmentId)
    {
        try
        {
            // 🔹 Hämta jämförelsedata för patient och personal
            var response = await _httpClient.GetAsync($"statistics/comparison-table-staff/{assessmentId}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Kunde inte hämta jämförelsedata. ({(int)response.StatusCode}) {errorMsg}";
                return RedirectToAction("Index", "StaffAssessment");
            }

            var json = await response.Content.ReadAsStringAsync();
            var comparison = JsonSerializer.Deserialize<List<StaffComparisonRowDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (comparison == null || !comparison.Any())
            {
                TempData["Error"] = "Inga jämförelsedata kunde tolkas eller bedömningen är tom.";
                return RedirectToAction("Index", "StaffAssessment");
            }

            // 🔍 Hämta bedömningen för att få UserId
            var assessmentResponse = await _httpClient.GetAsync($"assessment/{assessmentId}");
            if (!assessmentResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "Kunde inte hämta information om användaren.";
                return RedirectToAction("Index", "StaffAssessment");
            }

            var assessmentJson = await assessmentResponse.Content.ReadAsStringAsync();
            var assessment = JsonSerializer.Deserialize<AssessmentDto>(assessmentJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // 🧾 Skicka vidare information till vyn
            ViewBag.UserId = assessment?.UserId ?? 0;
            ViewBag.PatientName = comparison.First().Username;
            ViewBag.AssessmentDate = comparison.First().CreatedAt;

            return View("Comparison", comparison);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
            return RedirectToAction("Index", "StaffAssessment");
        }
    }


    // GET: /StaffStatistics/ChangeOverview/{userId}
    [HttpGet("ChangeOverview/{userId}")]
    public async Task<IActionResult> ChangeOverview(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"statistics/staff-change-overview/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Kunde inte hämta översiktsdata.";
                return RedirectToAction("Assessments", "StaffAssessment", new { userId });
            }

            var json = await response.Content.ReadAsStringAsync();
            if (json.Contains("inte tillräckligt"))
            {
                TempData["Error"] = "Det krävs minst två avslutade bedömningar för att visa förändringar.";
                return RedirectToAction("Assessments", "StaffAssessment", new { userId });
            }

            var overview = JsonSerializer.Deserialize<StaffChangeOverviewDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ViewBag.UserId = userId;
            return View("ChangeOverview", overview);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
            return RedirectToAction("Assessments", "StaffAssessment", new { userId });
        }
    }
}
