using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

[RoleAuthorize("patient")]
public class PatientResultController : Controller
{
    private readonly HttpClient _httpClient;

    public PatientResultController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("DLDA");
    }

    // GET: /PatientResult/Index?assessmentId=45
    public async Task<IActionResult> Index(int assessmentId)
    {
        try
        {
            var assessmentResponse = await _httpClient.GetAsync($"Assessment/{assessmentId}");
            if (!assessmentResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "Kunde inte hämta bedömningsinformation.";
                return View("Error");
            }

            var assessment = await assessmentResponse.Content.ReadFromJsonAsync<AssessmentDto>();

            var response = await _httpClient.GetAsync($"AssessmentItem/patient/assessment/{assessmentId}/overview");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Kunde inte hämta frågeöversikt.";
                return View("Error");
            }

            var overview = await response.Content.ReadFromJsonAsync<AssessmentOverviewDto>();
            if (overview == null)
            {
                TempData["Error"] = "Ingen data tillgänglig för sammanställning.";
                return View("Error");
            }

            return View(overview);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Tekniskt fel: {ex.Message}";
            return View("Error");
        }
    }

    // POST: /PatientResult/Complete
    [HttpPost]
    public async Task<IActionResult> Complete(int assessmentId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"AssessmentItem/assessment/{assessmentId}/complete", null);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Kunde inte markera bedömningen som klar.";
                return RedirectToAction("Index", new { assessmentId });
            }

            TempData["Success"] = "Bedömningen är nu markerad som klar.";
            return RedirectToAction("Index", "PatientAssessment");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
            return RedirectToAction("Index", new { assessmentId });
        }
    }

    // POST: /PatientResult/UpdateAnswer
    [HttpPost]
    public async Task<IActionResult> UpdateAnswer(int itemId, int assessmentId, int answer, string? comment)
    {
        try
        {
            var dto = new PatientAnswerDto
            {
                Answer = answer,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment
            };

            var response = await _httpClient.PutAsJsonAsync($"AssessmentItem/patient/{itemId}", dto);

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

        return RedirectToAction("Index", new { assessmentId });
    }
}
