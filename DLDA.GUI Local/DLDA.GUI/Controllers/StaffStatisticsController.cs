using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

//  Övergripande statistik, Matchningsgrad, Diff-tabeller, Trendanalys över tid

[Route("StaffStatistics")]
[RoleAuthorize("staff")]
public class StaffStatisticsController : Controller
{
    private readonly HttpClient _httpClient;

    public StaffStatisticsController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("DLDA");
    }

    // GET: /StaffStatistics/Comparison/82
    [HttpGet("Comparison/{assessmentId}")]
    public async Task<IActionResult> Comparison(int assessmentId)
    {
        try
        {
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

            // 🔍 Hämta bedömningen för att extrahera UserId (kan hämtas från separat API eller cache)
            var assessmentResponse = await _httpClient.GetAsync($"assessment/{assessmentId}");
            if (!assessmentResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "Kunde inte hämta användarinformation.";
                return RedirectToAction("Index", "StaffAssessment");
            }

            var assessmentJson = await assessmentResponse.Content.ReadAsStringAsync();
            var assessment = JsonSerializer.Deserialize<AssessmentDto>(assessmentJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ViewBag.UserId = assessment?.UserId ?? 0;

            return View("Comparison", comparison);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ett fel inträffade: {ex.Message}";
            return RedirectToAction("Index", "StaffAssessment");
        }
    }


}