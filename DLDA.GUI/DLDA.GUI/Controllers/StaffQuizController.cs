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

    // GET: /StaffQuiz/Resume?assessmentId=66&userId=6
    [HttpGet("StaffQuiz/Resume")]
    public async Task<IActionResult> Resume(int assessmentId, int userId)
    {
        Console.WriteLine($"[STAFF QUIZ] Resume: assessmentId={assessmentId}, userId={userId}");

        var response = await _httpClient.GetAsync($"Question/quiz/staff/next/{assessmentId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            TempData["Success"] = "Du har gått igenom alla frågor.";
            Console.WriteLine($"[REDIRECT] Alla frågor klara. Skickar vidare till resultatvyn för bedömning {assessmentId}");
            return RedirectToAction("Index", "StaffResult", new { id = assessmentId });
        }

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Kunde inte hämta nästa fråga.";
            return RedirectToAction("Index", "StaffAssessment");
        }

        var question = await response.Content.ReadFromJsonAsync<StaffQuestionDto>();
        return View("Question", question);
    }



    // POST: /StaffQuiz/SubmitAnswer
    [HttpPost]
    public async Task<IActionResult> SubmitAnswer(int itemId, int assessmentId, int answer, string? comment, bool flag, int userId)
    {
        Console.WriteLine($"[DEBUG] SubmitAnswer: userId={userId}");

        var dto = new SubmitStaffAnswerDto
        {
            ItemID = itemId,
            Answer = answer,
            Comment = comment,
            Flag = flag
        };

        var response = await _httpClient.PostAsJsonAsync("Question/quiz/staff/submit", dto);

        return RedirectToAction("Resume", new { assessmentId, userId });
    }

    // POST: /StaffQuiz/Previous
    [HttpPost]
    public async Task<IActionResult> Previous(int assessmentId, int currentOrder, int userId)
    {
        Console.WriteLine($"[STAFF QUIZ] Previous: assessmentId={assessmentId}, order={currentOrder}");

        var response = await _httpClient.GetAsync($"Question/quiz/staff/previous/{assessmentId}/{currentOrder}");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Kunde inte hämta föregående fråga.";
            return RedirectToAction("Resume", new { assessmentId, userId });
        }

        var question = await response.Content.ReadFromJsonAsync<StaffQuestionDto>();
        return View("Question", question);
    }

    // POST: /StaffQuiz/Pause
    [HttpPost("StaffQuiz/Pause")]
    public IActionResult Pause(int assessmentId, int userId)
    {
        TempData["Info"] = "Bedömningen är pausad. Du kan återuppta den senare.";
        return RedirectToAction("Assessments", "StaffAssessment", new { userId });
    }

}
