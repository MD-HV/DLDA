using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

[Route("PatientStatistics")]
[RoleAuthorize("patient")]
public class PatientStatisticsController : Controller
{
    private readonly HttpClient _httpClient;

    public PatientStatisticsController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("DLDA");
    }

    // GET: /PatientStatistics/Single/82
    [HttpGet("Single/{assessmentId}")]
    public async Task<IActionResult> Single(int assessmentId)
    {
        // Hämta alla svar från API:t
        var response = await _httpClient.GetAsync($"AssessmentItem/patient/assessment/{assessmentId}");
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Kunde inte hämta bedömningssvar.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        var answers = await response.Content.ReadFromJsonAsync<List<PatientAnswerStatsDto>>();
        if (answers == null)
        {
            TempData["Error"] = "Inga svar kunde läsas in.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        // Hämta metainfo om bedömningen
        var assessmentResp = await _httpClient.GetAsync($"Assessment/{assessmentId}");
        var assessment = await assessmentResp.Content.ReadFromJsonAsync<AssessmentDto>();

        var model = new PatientStatisticsDto
        {
            AssessmentId = assessmentId,
            CreatedAt = assessment?.CreatedAt ?? DateTime.MinValue,
            Answers = answers
        };

        return View("Single", model);
    }


    // Tillåt både: ?assessmentId=82 och /Overview/82
    [HttpGet("Overview")]
    [HttpGet("Overview/{assessmentId}")]
    public async Task<IActionResult> Overview(int assessmentId)
    {
        var response = await _httpClient.GetAsync($"statistics/summary/patient/{assessmentId}");
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Kunde inte hämta statistik.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        var summary = await response.Content.ReadFromJsonAsync<PatientSingleSummaryDto>();
        return View("Single", summary);
    }

    [HttpGet("Improvement/{userId}")]
    public async Task<IActionResult> Improvement(int userId)
    {
        var response = await _httpClient.GetAsync($"statistics/patient-change-overview/{userId}");
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Det gick inte att hämta förbättringsdata.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        // 🟡 Läs svaret som ren text
        var jsonString = await response.Content.ReadAsStringAsync();

        // 🔴 Om det är ett felmeddelande (t.ex. från NotFound), returnera tidigt
        if (jsonString.Contains("inte tillräckligt"))
        {
            TempData["Error"] = "Du måste ha minst två avslutade bedömningar för att visa förbättringar över tid.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        // ✅ Om inte, deserialisera till DTO
        var data = JsonSerializer.Deserialize<PatientChangeOverviewDto>(jsonString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return View("Improvement", data);
    }

}
