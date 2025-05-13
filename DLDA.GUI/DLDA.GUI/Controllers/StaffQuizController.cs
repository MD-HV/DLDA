using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
        var response = await _httpClient.GetAsync($"Question/quiz/staff/next/{assessmentId}");

        Console.WriteLine($"[DEBUG] StatusCode: {response.StatusCode}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            // 👉 Dirigera vidare till sammanställning
            TempData["Success"] = "Du har gått igenom alla frågor.";
            return RedirectToAction("Index", "StaffResult", new { id = assessmentId });
        }

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Kunde inte läsa in frågan.";
            return RedirectToAction("Index", "StaffAssessment");
        }

        var dto = await response.Content.ReadFromJsonAsync<StaffQuestionDto>();
        if (dto == null)
        {
            TempData["Error"] = "Kunde inte läsa in frågan.";
            return RedirectToAction("Index", "StaffAssessment");
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
