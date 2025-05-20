using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs.Patient;  
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    /// <summary>
    /// Controller för quizflöde där patienten besvarar frågor i en bedömning.
    /// </summary>
    [RoleAuthorize("patient")]
    public class PatientQuizController : Controller
    {
        private readonly PatientQuizService _service;

        public PatientQuizController(PatientQuizService service)
        {
            _service = service;
        }

        /// <summary>
        /// Visar info-sida innan quiz startas (för att visa påbörjad status).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Info(int id)
        {
            var assessment = await _service.GetAssessmentAsync(id);
            if (assessment == null)
            {
                TempData["Error"] = "Kunde inte hämta information om bedömningen.";
                return View("Error");
            }

            ViewBag.AssessmentId = id;
            ViewBag.HasStarted = assessment.HasStarted;
            return View();
        }

        /// <summary>
        /// Visar val av skattningsskala för aktuell bedömning.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ScaleSelect(int id)
        {
            var assessment = await _service.GetAssessmentAsync(id);
            if (assessment == null) return View("Error");
            return View(assessment);
        }

        /// <summary>
        /// Sparar vald skattningsskala och fortsätter till quiz.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SetScale(int id, string selectedScale)
        {
            var success = await _service.UpdateScaleAsync(id, selectedScale);
            if (!success)
            {
                TempData["Error"] = "Kunde inte spara vald skala.";
                return RedirectToAction("ScaleSelect", new { id });
            }

            return RedirectToAction("Resume", new { id });
        }

        /// <summary>
        /// Återupptar quiz och laddar nästa obesvarade fråga.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Resume(int id)
        {
            var assessment = await _service.GetAssessmentAsync(id);
            if (assessment == null) return View("Error");

            if (assessment.IsComplete)
                return RedirectToAction("Index", "PatientResult", new { assessmentId = id });

            var question = await _service.GetNextQuestionAsync(id);
            if (question == null)
            {
                TempData["Error"] = "Du har svarat på alla frågor.";
                return RedirectToAction("Index", "PatientResult", new { assessmentId = id });
            }

            var totalQuestions = await _service.GetTotalQuestionCountAsync(id);
            ViewBag.TotalQuestions = totalQuestions ?? 0; // Fallback: 0 om fel
            ViewBag.AssessmentId = id;

            return View("Question", question);
        }

        /// <summary>
        /// Skickar in svar på en fråga.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(int itemId, int assessmentId, int answer, string? comment)
        {
            var dto = new PatientAnswerDto
            {
                Answer = answer,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment
            };

            var success = await _service.SubmitAnswerAsync(itemId, dto);
            if (!success)
            {
                TempData["Error"] = "Kunde inte spara svaret.";
            }

            return RedirectToAction("Resume", new { id = assessmentId });
        }

        /// <summary>
        /// Markerar en fråga som överhoppad och går vidare.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SkipQuestion(int itemId, int assessmentId)
        {
            var success = await _service.SkipQuestionAsync(itemId);
            if (!success)
            {
                TempData["Error"] = "Kunde inte hoppa över frågan.";
            }

            return RedirectToAction("Resume", new { id = assessmentId });
        }

        /// <summary>
        /// Hämtar föregående fråga baserat på nuvarande ordning.
        /// </summary>
        public async Task<IActionResult> Previous(int assessmentId, int currentOrder)
        {
            var question = await _service.GetPreviousQuestionAsync(assessmentId, currentOrder);
            if (question == null)
            {
                TempData["Error"] = "Kunde inte hämta föregående fråga.";
                return RedirectToAction("Resume", new { id = assessmentId });
            }

            var totalQuestions = await _service.GetTotalQuestionCountAsync(assessmentId);
            ViewBag.TotalQuestions = totalQuestions ?? 0; // fallback om något går fel
            ViewBag.AssessmentId = assessmentId;

            return View("Question", question);
        }

        /// <summary>
        /// Pausar quiz och återvänder till bedömningsöversikten.
        /// </summary>
        [HttpPost]
        public IActionResult Pause(int assessmentId)
        {
            TempData["Success"] = "Du kan återuppta din bedömning senare.";
            return RedirectToAction("Index", "PatientAssessment");
        }
    }
}
