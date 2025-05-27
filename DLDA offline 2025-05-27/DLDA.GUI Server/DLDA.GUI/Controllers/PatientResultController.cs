using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs.Patient;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    /// <summary>
    /// Controller som hanterar sammanställning och uppföljning av patientens bedömning.
    /// </summary>
    [RoleAuthorize("patient")]
    public class PatientResultController : Controller
    {
        private readonly PatientResultService _service;

        public PatientResultController(PatientResultService service)
        {
            _service = service;
        }

        /// <summary>
        /// Visar sammanställning över svar för en given bedömning.
        /// </summary>
        public async Task<IActionResult> Index(int assessmentId)
        {
            var assessment = await _service.GetAssessmentAsync(assessmentId);
            if (assessment == null)
            {
                TempData["Error"] = "Kunde inte hämta bedömningsinformation.";
                return View("Error");
            }

            var overview = await _service.GetOverviewAsync(assessmentId);
            if (overview == null)
            {
                TempData["Error"] = "Kunde inte hämta frågeöversikt.";
                return View("Error");
            }

            return View(overview);
        }

        /// <summary>
        /// Markerar att bedömningen är komplett och återgår till översikten.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Complete(int assessmentId)
        {
            var success = await _service.CompleteAssessmentAsync(assessmentId);
            TempData[success ? "Success" : "Error"] = success
                ? "Bedömningen är nu markerad som klar."
                : "Kunde inte markera bedömningen som klar.";

            return success
                ? RedirectToAction("Index", "PatientAssessment")
                : RedirectToAction("Index", new { assessmentId });
        }

        /// <summary>
        /// Uppdaterar ett tidigare svar i efterhand.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateAnswer(int itemId, int assessmentId, int answer, string? comment)
        {
            var dto = new PatientAnswerDto
            {
                Answer = answer,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment
            };

            var success = await _service.UpdateAnswerAsync(itemId, dto);
            TempData[success ? "Success" : "Error"] = success
                ? "Svar uppdaterat."
                : "Kunde inte spara ändringar.";

            return RedirectToAction("Index", new { assessmentId });
        }
    }
}
