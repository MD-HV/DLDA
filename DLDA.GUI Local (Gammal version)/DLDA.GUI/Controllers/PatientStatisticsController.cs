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
        try
        {
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

            var assessmentResp = await _httpClient.GetAsync($"Assessment/{assessmentId}");
            if (!assessmentResp.IsSuccessStatusCode)
            {
                TempData["Error"] = "Kunde inte hämta bedömningsinformation.";
                return RedirectToAction("Index", "PatientAssessment");
            }

            var assessment = await assessmentResp.Content.ReadFromJsonAsync<AssessmentDto>();

            var model = new PatientStatisticsDto
            {
                AssessmentId = assessmentId,
                CreatedAt = assessment?.CreatedAt ?? DateTime.MinValue,
                Answers = answers
            };

            return View("Single", model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Tekniskt fel: {ex.Message}";
            return RedirectToAction("Index", "PatientAssessment");
        }
    }

    // GET: /PatientStatistics/Overview or /PatientStatistics/Overview/82
    [HttpGet("Overview")]
    [HttpGet("Overview/{assessmentId}")]
    public async Task<IActionResult> Overview(int assessmentId)
    {
        try
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
        catch (Exception ex)
        {
            TempData["Error"] = $"Tekniskt fel: {ex.Message}";
            return RedirectToAction("Index", "PatientAssessment");
        }
    }

    // GET: /PatientStatistics/Improvement/{userId}
    [HttpGet("Improvement/{userId}")]
    public async Task<IActionResult> Improvement(int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"statistics/patient-change-overview/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Det gick inte att hämta förbättringsdata.";
                return RedirectToAction("Index", "PatientAssessment");
            }

            var jsonString = await response.Content.ReadAsStringAsync();

            if (jsonString.Contains("inte tillräckligt"))
            {
                TempData["Error"] = "Du måste ha minst två avslutade bedömningar för att visa förbättringar över tid.";
                return RedirectToAction("Index", "PatientAssessment");
            }

            var data = JsonSerializer.Deserialize<PatientChangeOverviewDto>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View("Improvement", data);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
            return RedirectToAction("Index", "PatientAssessment");
        }
    }
}
