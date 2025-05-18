using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    /// <summary>
    /// Controller för personalens hantering av bedömningar.
    /// </summary>
    [RoleAuthorize("staff")]
    public class StaffAssessmentController : Controller
    {
        private readonly StaffAssessmentService _service;

        public StaffAssessmentController(StaffAssessmentService service)
        {
            _service = service;
        }

        /// <summary>
        /// Visar alla patienter med senaste bedömning.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var patients = await _service.GetPatientsWithLatestAsync();
            return View(patients);
        }

        /// <summary>
        /// Visar alla bedömningar för en specifik patient.
        /// </summary>
        public async Task<IActionResult> Assessments(int userId)
        {
            ViewBag.UserId = userId;
            ViewBag.Username = await _service.GetUsernameAsync(userId);

            var assessments = await _service.GetAssessmentsForUserAsync(userId);
            return View("Assessments", assessments);
        }

        /// <summary>
        /// Skapar en ny bedömning åt angiven patient.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateForPatient(int userId)
        {
            var success = await _service.CreateAssessmentAsync(userId);

            TempData[success ? "Success" : "Error"] = success
                ? "Ny bedömning skapades."
                : "Misslyckades att skapa bedömning.";

            return RedirectToAction("Assessments", new { userId });
        }

        /// <summary>
        /// Visar bekräftelsesida innan radering av bedömning.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var assessment = await _service.GetAssessmentAsync(id);
            if (assessment == null) return View("Error");

            return View("Delete", assessment);
        }

        /// <summary>
        /// Raderar angiven bedömning efter bekräftelse.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int userId)
        {
            var success = await _service.DeleteAssessmentAsync(id);

            TempData[success ? "Success" : "Error"] = success
                ? "Bedömning togs bort."
                : "Misslyckades att ta bort bedömning.";

            return RedirectToAction("Assessments", new { userId });
        }
    }
}
