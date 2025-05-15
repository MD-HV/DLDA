using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;

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
            // Hämta bedömningen (för att få IsComplete)
            var assessmentResponse = await _httpClient.GetAsync($"Assessment/{assessmentId}");
            if (!assessmentResponse.IsSuccessStatusCode)
                return View("Error");

            var assessment = await assessmentResponse.Content.ReadFromJsonAsync<AssessmentDto>();

            // Hämta frågor + svar
            var response = await _httpClient.GetAsync($"AssessmentItem/patient/assessment/{assessmentId}/overview");
            if (!response.IsSuccessStatusCode)
                return View("Error");

            var overview = await response.Content.ReadFromJsonAsync<AssessmentOverviewDto>();
            if (overview == null)
                return View("Error");

            return View(overview);
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
            return View("Error");
        }
    }

    // POST: /PatientResult/Complete
    [HttpPost]
    public async Task<IActionResult> Complete(int assessmentId)
    {
        var response = await _httpClient.PostAsync($"AssessmentItem/assessment/{assessmentId}/complete", null);
        if (!response.IsSuccessStatusCode)
            return View("Error");

        TempData["Success"] = "Bedömningen är nu markerad som klar.";
        return RedirectToAction("Index", "PatientAssessment");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAnswer(int itemId, int assessmentId, int answer, string? comment)
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

        return RedirectToAction("Index", new { assessmentId });
    }
}
