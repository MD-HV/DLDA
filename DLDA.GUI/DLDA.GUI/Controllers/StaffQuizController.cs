using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;

[RoleAuthorize("staff")]
public class StaffQuizController : Controller
{
    private readonly HttpClient _httpClient;

    public StaffQuizController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("DLDA");
    }

    // GET: /StaffQuiz/Resume/{assessmentId}
    [HttpGet]
    public async Task<IActionResult> Resume(int assessmentId)
    {
        // 1. Hämta aktuell bedömning
        var assessmentResponse = await _httpClient.GetAsync($"Assessment/{assessmentId}");
        if (!assessmentResponse.IsSuccessStatusCode)
            return View("Error");

        var assessment = await assessmentResponse.Content.ReadFromJsonAsync<AssessmentDto>();

        // 2. Om personalen redan är klar, visa resultat
        if (assessment?.IsStaffComplete == true)
        {
            TempData["Success"] = "Personalen har redan slutfört denna bedömning.";
            return RedirectToAction("StaffView", "Result", new { id = assessmentId });
        }

        // 3. Hämta nästa obesvarade fråga
        var response = await _httpClient.GetAsync($"Question/quiz/staff/next/{assessmentId}");
        Console.WriteLine($"[DEBUG] StatusCode: {response.StatusCode}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            TempData["Success"] = "Du har gått igenom alla frågor.";
            return RedirectToAction("Assessments", "StaffAssessment", new { userId = assessment?.UserId });
        }

        if (!response.IsSuccessStatusCode)
            return View("Error");

        var dto = await response.Content.ReadFromJsonAsync<StaffQuestionDto>();
        if (dto == null)
        {
            TempData["Error"] = "Kunde inte läsa in frågan.";
            return View("Error");
        }

        ViewBag.AssessmentId = assessmentId;
        return View("Question", dto);
    }

    // POST: /StaffQuiz/SubmitAnswer
    [HttpPost]
    public async Task<IActionResult> SubmitAnswer(int itemId, int assessmentId, int answer, string? comment, bool flag)
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
            TempData["Error"] = "Kunde inte spara svar.";
        }

        return RedirectToAction("Resume", new { assessmentId });
    }
}
