using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs.Staff;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    /// <summary>
    /// Controller för personalens frågeflöde under bedömning.
    /// </summary>
    [RoleAuthorize("staff")]
    public class StaffQuizController : Controller
    {
        private readonly StaffQuizService _service;

        public StaffQuizController(StaffQuizService service)
        {
            _service = service;
        }

        /// <summary>
        /// Återupptar personalens bedömning – visar nästa fråga.
        /// </summary>
        [HttpGet("StaffQuiz/Resume")]
        public async Task<IActionResult> Resume(int assessmentId, int userId)
        {
            var question = await _service.GetNextQuestionAsync(assessmentId);

            if (question == null)
            {
                TempData["Success"] = "Du har gått igenom alla frågor.";
                return RedirectToAction("Index", "StaffResult", new { id = assessmentId });
            }

            var totalQuestions = await _service.GetTotalQuestionCountForStaffAsync(assessmentId);

            ViewBag.AssessmentId = assessmentId;
            ViewBag.UserId = userId;
            ViewBag.TotalQuestions = totalQuestions ?? 0;

            return View("Question", question);
        }

        /// <summary>
        /// Skickar in personalens svar, kommentar och flagga för en fråga.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(int itemId, int assessmentId, int answer, string? comment, bool flag, int userId)
        {
            var dto = new SubmitStaffAnswerDto
            {
                ItemID = itemId,
                Answer = answer,
                Comment = comment,
                Flag = flag
            };

            var success = await _service.SubmitAnswerAsync(dto);
            if (!success)
                TempData["Error"] = "Kunde inte spara svaret.";

            return RedirectToAction("Resume", new { assessmentId, userId });
        }

        /// <summary>
        /// Hämtar föregående fråga i personalens frågeflöde.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Previous(int assessmentId, int currentOrder, int userId)
        {
            var question = await _service.GetPreviousQuestionAsync(assessmentId, currentOrder);

            if (question == null)
            {
                TempData["Error"] = "Kunde inte hämta föregående fråga.";
                return RedirectToAction("Resume", new { assessmentId, userId });
            }

            var totalQuestions = await _service.GetTotalQuestionCountForStaffAsync(assessmentId);

            ViewBag.AssessmentId = assessmentId;
            ViewBag.UserId = userId;
            ViewBag.TotalQuestions = totalQuestions ?? 0;

            return View("Question", question);
        }


        /// <summary>
        /// Pausar bedömningen och går tillbaka till översikten.
        /// </summary>
        [HttpPost("StaffQuiz/Pause")]
        public IActionResult Pause(int assessmentId, int userId)
        {
            TempData["Info"] = "Bedömningen är pausad. Du kan återuppta den senare.";
            return RedirectToAction("Assessments", "StaffAssessment", new { userId });
        }

        /// <summary>
        /// Hoppar över aktuell fråga i personalens bedömning.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SkipQuestion(int itemId, int assessmentId, string? comment, bool flag, int userId)
        {
            var success = await _service.SkipQuestionAsync(itemId, comment, flag);

            if (!success)
                TempData["Error"] = "Kunde inte hoppa över frågan.";

            return RedirectToAction("Resume", new { assessmentId, userId });
        }
    }
}
