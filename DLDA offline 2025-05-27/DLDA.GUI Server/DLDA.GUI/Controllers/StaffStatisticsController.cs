using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using DLDA.GUI.DTOs.Staff;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller för personalens statistikvyer, såsom jämförelse och förändringar över tid.
/// </summary>
[Route("StaffStatistics")]
[RoleAuthorize("staff")]
public class StaffStatisticsController : Controller
{
    private readonly StaffStatisticsService _service;

    public StaffStatisticsController(StaffStatisticsService service)
    {
        _service = service;
    }

    [HttpGet("Comparison/{assessmentId}")]
    public async Task<IActionResult> Comparison(int assessmentId)
    {
        try
        {
            var result = await _service.GetComparisonAsync(assessmentId);
            var comparison = result.Comparison;
            var assessment = result.Assessment;

            if (assessment == null)
            {
                TempData["Error"] = "Bedömningen kunde inte hittas.";
                return RedirectToAction("Index", "StaffAssessment");
            }

            int userId = assessment.UserId; // 👈 nu har vi userId säkert

            if (comparison == null || !comparison.Any())
            {
                TempData["Error"] = "Jämförelsen kan inte visas eftersom patienten eller personalen inte har svarat på några frågor i denna bedömning.";
                return RedirectToAction("Assessments", "StaffAssessment", new { userId });
            }

            ViewBag.UserId = userId;
            ViewBag.AssessmentId = assessment.AssessmentID;
            ViewBag.PatientName = comparison.First().Username;
            ViewBag.AssessmentDate = comparison.First().CreatedAt;

            return View("Comparison", comparison);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
            return RedirectToAction("Assessments", "StaffAssessment");
        }
    }



    /// <summary>
    /// Visar förbättringar och försämringar över tid för patientens bedömningar.
    /// </summary>
    [HttpGet("ChangeOverview/{userId}")]
    public async Task<IActionResult> ChangeOverview(int userId)
    {
        try
        {
            var overview = await _service.GetChangeOverviewAsync(userId);

            if (overview == null)
            {
                TempData["Error"] = "Det finns inte tillräckligt med svar i bedömningarna för att visa en jämförelse över tid.";
                return RedirectToAction("Assessments", "StaffAssessment", new { userId });
            }

            ViewBag.UserId = userId;
            return View("ChangeOverview", overview);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
            return RedirectToAction("Assessments", "StaffAssessment", new { userId });
        }
    }

    /// <summary>
    /// Visar patientens egen svarsfördelning i en piechart.
    /// </summary>
    [HttpGet("PatientAnswerSummary/{assessmentId}")]
    public async Task<IActionResult> PatientAnswerSummary(int assessmentId)
    {
        try
        {
            var result = await _service.GetComparisonAsync(assessmentId);
            var data = result.Comparison;
            var assessment = result.Assessment;

            if (data == null || !data.Any() || assessment == null)
            {
                TempData["Error"] = "Kunde inte hämta patientens svar.";
                return RedirectToAction("Comparison", new { assessmentId });
            }

            var first = data.First();

            ViewBag.PatientName = first.Username;
            ViewBag.AssessmentDate = first.CreatedAt;
            ViewBag.UserId = assessment.UserId;
            ViewBag.AssessmentId = assessment.AssessmentID;

            return View("PatientAnswerSummary", data);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
            return RedirectToAction("Comparison", new { assessmentId });
        }
    }

    /// <summary>
    /// Jämför två avslutade personalbedömningar för en patient och visar förändringar över tid.
    /// </summary>
    [HttpPost("Compare")]
    public async Task<IActionResult> Compare(int userId, int firstId, int secondId)
    {
        if (firstId == secondId)
        {
            TempData["Error"] = "Du måste välja två olika bedömningar att jämföra.";
            return RedirectToAction("Assessments", "StaffAssessment", new { userId });
        }

        var result = await _service.CompareAssessmentsAsync(firstId, secondId);
        if (result == null)
        {
            TempData["Error"] = "Jämförelsen kunde inte göras. Kontrollera att båda bedömningarna har tillräckligt med svar.";
            return RedirectToAction("Assessments", "StaffAssessment", new { userId });
        }

        ViewBag.UserId = userId;
        return View("ChangeOverview", result); 
    }
}
