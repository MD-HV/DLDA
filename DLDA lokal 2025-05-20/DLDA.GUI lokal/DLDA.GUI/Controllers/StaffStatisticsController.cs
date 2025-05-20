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

    /// <summary>
    /// Visar jämförelse mellan patientens och personalens svar för en specifik bedömning.
    /// </summary>
    [HttpGet("Comparison/{assessmentId}")]
    public async Task<IActionResult> Comparison(int assessmentId)
    {
        try
        {
            // 🧠 Hämta data via tjänst
            var result = await _service.GetComparisonAsync(assessmentId);
            var comparison = result.Comparison;
            var assessment = result.Assessment;

            // ❌ Kontrollera om data saknas
            if (comparison == null || !comparison.Any() || assessment == null)
            {
                TempData["Error"] = "Kunde inte hämta jämförelsedata eller bedömning.";
                return RedirectToAction("Index", "StaffAssessment");
            }

            // ✅ Förbered data till vyn
            ViewBag.UserId = assessment.UserId;
            ViewBag.AssessmentId = assessment.AssessmentID;
            ViewBag.PatientName = comparison.First().Username;
            ViewBag.AssessmentDate = comparison.First().CreatedAt;

            return View("Comparison", comparison);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ett tekniskt fel uppstod: {ex.Message}";
            return RedirectToAction("Index", "StaffAssessment");
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
                TempData["Error"] = "Kunde inte hämta översiktsdata.";
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
}
