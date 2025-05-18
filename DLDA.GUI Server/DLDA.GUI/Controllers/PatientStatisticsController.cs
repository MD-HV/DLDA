using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs.Patient;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

[Route("PatientStatistics")]
[RoleAuthorize("patient")]
public class PatientStatisticsController : Controller
{
    private readonly PatientStatisticsService _service;

    public PatientStatisticsController(PatientStatisticsService service)
    {
        _service = service;
    }

    /// <summary>
    /// Visar statistik för en enskild bedömning (rådata per fråga).
    /// </summary>
    [HttpGet("Single/{assessmentId}")]
    public async Task<IActionResult> Single(int assessmentId)
    {
        var answers = await _service.GetAnswersForAssessmentAsync(assessmentId);
        if (answers.Count == 0)
        {
            TempData["Error"] = "Inga svar kunde läsas in.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        var assessment = await _service.GetAssessmentAsync(assessmentId);
        if (assessment == null)
        {
            TempData["Error"] = "Kunde inte hämta bedömningsinformation.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        var model = new PatientStatisticsDto
        {
            AssessmentId = assessmentId,
            CreatedAt = assessment.CreatedAt,
            Answers = answers
        };

        return View("Single", model);
    }

    /// <summary>
    /// Visar sammanfattande statistik för en bedömning (sammanställd vy).
    /// </summary>
    [HttpGet("Overview")]
    [HttpGet("Overview/{assessmentId}")]
    public async Task<IActionResult> Overview(int assessmentId)
    {
        var summary = await _service.GetSummaryAsync(assessmentId);
        if (summary == null)
        {
            TempData["Error"] = "Kunde inte hämta statistik.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        return View("Single", summary);
    }

    /// <summary>
    /// Visar förbättringar över tid (kräver minst två avslutade bedömningar).
    /// </summary>
    [HttpGet("Improvement/{userId}")]
    public async Task<IActionResult> Improvement(int userId)
    {
        var data = await _service.GetImprovementDataAsync(userId);
        if (data == null)
        {
            TempData["Error"] = "Du måste ha minst två avslutade bedömningar för att visa förbättringar över tid.";
            return RedirectToAction("Index", "PatientAssessment");
        }

        return View("Improvement", data);
    }
}
