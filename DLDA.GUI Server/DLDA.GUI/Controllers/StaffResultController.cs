using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs.Staff;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    /// <summary>
    /// Controller som hanterar personalens resultatöversikt för bedömningar.
    /// </summary>
    [RoleAuthorize("staff")]
    public class StaffResultController : Controller
    {
        private readonly StaffResultService _service;

        public StaffResultController(StaffResultService service)
        {
            _service = service;
        }

        /// <summary>
        /// Visar personalens sammanställning av en bedömning.
        /// </summary>
        public async Task<IActionResult> Index(int id)
        {
            var overview = await _service.GetOverviewAsync(id);

            if (overview == null)
            {
                TempData["Error"] = "Kunde inte hämta personalsammanställning.";
                return RedirectToAction("Index", "StaffAssessment");
            }

            return View("Index", overview);
        }

        /// <summary>
        /// Uppdaterar ett personal-svar, kommentar och flagga i sammanställningen.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateStaffAnswer(int itemId, int assessmentId, int answer, string? comment, bool flag)
        {
            var dto = new SubmitStaffAnswerDto
            {
                ItemID = itemId,
                Answer = answer,
                Comment = comment,
                Flag = flag
            };

            var success = await _service.UpdateStaffAnswerAsync(dto);
            TempData[success ? "Success" : "Error"] = success
                ? "Svar uppdaterat."
                : "Kunde inte spara ändringar.";

            return RedirectToAction("Index", new { id = assessmentId });
        }

        /// <summary>
        /// Markerar personalens bedömning som komplett.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Complete(int assessmentId, int userId)
        {
            var success = await _service.CompleteStaffAssessmentAsync(assessmentId);
            TempData[success ? "Success" : "Error"] = success
                ? "Personalens bedömning har markerats som klar."
                : "Kunde inte markera bedömningen som klar. Kontrollera att alla frågor är besvarade.";

            return success
                ? RedirectToAction("Assessments", "StaffAssessment", new { userId })
                : RedirectToAction("Index", new { id = assessmentId });
        }

        /// <summary>
        /// Låser upp en bedömning som tidigare markerats som klar.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Unlock(int assessmentId, int userId)
        {
            var success = await _service.UnlockAssessmentAsync(assessmentId);
            TempData[success ? "Success" : "Error"] = success
                ? "Bedömningen har låsts upp."
                : "Misslyckades med att låsa upp bedömningen.";

            return success
                ? RedirectToAction("Index", new { id = assessmentId })
                : RedirectToAction("Assessments", "StaffAssessment", new { userId });
        }
    }
}
