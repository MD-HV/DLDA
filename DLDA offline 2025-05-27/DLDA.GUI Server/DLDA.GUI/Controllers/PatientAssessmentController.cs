using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DLDA.GUI.Controllers
{
    /// <summary>
    /// Controller för att visa patientens egna bedömningar.
    /// </summary>
    [RoleAuthorize("patient")]
    public class PatientAssessmentController : Controller
    {
        private readonly PatientAssessmentService _service;
        private readonly ILogger<PatientAssessmentController> _logger;

        public PatientAssessmentController(PatientAssessmentService service, ILogger<PatientAssessmentController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Visar en lista med alla patientens tidigare bedömningar.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                _logger.LogWarning("Ingen inloggad användare – redirect till login.");
                return RedirectToAction("Login", "Account");
            }

            var assessments = await _service.GetAssessmentsForUserAsync(userId.Value);
            return View(assessments);
        }
    }
}
