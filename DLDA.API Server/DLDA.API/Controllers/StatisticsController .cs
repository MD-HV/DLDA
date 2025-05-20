using DLDA.API.Data;
using DLDA.API.DTOs;
using DLDA.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DLDA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        // --------------------------
        // [PATIENT] – Återkoppling & översikt
        // --------------------------

        // GET: api/statistics/skipped/patient/{assessmentId}
        // Returnerar frågor där patienten inte svarat
        [HttpGet("skipped/patient/{assessmentId}")]
        public ActionResult<object> GetSkippedByPatient(int assessmentId)
        {
            var skipped = _context.AssessmentItems
                .Where(i => i.AssessmentID == assessmentId && !i.PatientAnswer.HasValue)
                .Include(i => i.Question)
                .ToList();

            return Ok(new
            {
                SkippedCount = skipped.Count,
                Questions = skipped
                    .Where(i => i.Question != null)
                    .Select(i => i.Question!.QuestionText)
                    .ToList()
            });
        }

        // GET: api/statistics/summary/{assessmentId}
        // Returnerar summering av patientens svar i en bedömning
        [HttpGet("summary/{assessmentId}")]
        public ActionResult<object> GetSingleAssessmentSummary(int assessmentId)
        {
            var items = _context.AssessmentItems
                .Where(i => i.AssessmentID == assessmentId && i.PatientAnswer.HasValue)
                .ToList();

            if (!items.Any())
                return BadRequest("Bedömningen saknar besvarade frågor och kan inte sammanfattas.");

            int noProblem = items.Count(i => i.PatientAnswer is 0 or 1);
            int someProblem = items.Count(i => i.PatientAnswer is 2 or 3);
            int bigProblem = items.Count(i => i.PatientAnswer == 4);
            int skipped = _context.AssessmentItems.Count(i => i.AssessmentID == assessmentId && !i.PatientAnswer.HasValue);

            var topProblems = items
                .Where(i => i.PatientAnswer >= 2)
                .OrderByDescending(i => i.PatientAnswer)
                .Take(5)
                .Select(i => new
                {
                    Question = i.Question?.QuestionText ?? "(fråga saknas)",
                    Answer = i.PatientAnswer
                });

            return Ok(new
            {
                TotalAnswered = items.Count,
                NoProblem = noProblem,
                SomeProblem = someProblem,
                BigProblem = bigProblem,
                Skipped = skipped,
                TopProblems = topProblems
            });
        }

        // GET: api/statistics/summary/patient/{assessmentId}
        // Returnerar patientens svar som DTO för statistikvisning
        [HttpGet("summary/patient/{assessmentId}")]
        public ActionResult<PatientSingleSummaryDto> GetSingleSummary(int assessmentId)
        {
            try
            {
                var items = _context.AssessmentItems
                    .Where(i => i.AssessmentID == assessmentId)
                    .Include(i => i.Question)
                    .ToList();

                var assessment = _context.Assessments
                    .FirstOrDefault(a => a.AssessmentID == assessmentId);

                if (assessment == null)
                    return NotFound("Bedömningen innehåller inga besvarade frågor.");

                var summary = new PatientSingleSummaryDto
                {
                    AssessmentId = assessmentId,
                    CreatedAt = assessment.CreatedAt ?? DateTime.MinValue,
                    Answers = items.Select(i => new PatientAnswerStatsDto
                    {
                        QuestionId = i.QuestionID,
                        QuestionText = i.Question?.QuestionText ?? "[Okänd fråga]",
                        Answer = i.PatientAnswer
                    }).ToList()
                };

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ett internt fel uppstod: {ex.Message}");
            }
        }

        // --------------------------
        // [STAFF] – Matchning och jämförelse
        // --------------------------

        // GET: api/statistics/match/{assessmentId}
        // Returnerar antal och procentuell matchning mellan patient och personal
        [HttpGet("match/{assessmentId}")]
        public ActionResult<object> GetMatchStatistics(int assessmentId)
        {
            var items = _context.AssessmentItems
                .Where(i => i.AssessmentID == assessmentId &&
                            i.PatientAnswer.HasValue && i.StaffAnswer.HasValue)
                .ToList();

            if (!items.Any())
                return BadRequest("Inga jämförbara svar hittades mellan patient och personal.");

            int matchCount = items.Count(i => i.PatientAnswer == i.StaffAnswer);
            int total = items.Count;

            return Ok(new
            {
                QuestionsCompared = total,
                Matches = matchCount,
                MatchPercent = Math.Round((double)matchCount / total * 100, 1),
                MismatchPercent = Math.Round(100 - ((double)matchCount / total * 100), 1)
            });
        }

        // GET: api/statistics/comparison-table-staff/{assessmentId}
        // Returnerar rad-för-rad jämförelse mellan patient och personal
        [HttpGet("comparison-table-staff/{assessmentId}")]
        public ActionResult<List<StaffComparisonRowDto>> GetAssessmentComparisonForStaff(int assessmentId)
        {
            var assessment = _context.Assessments
                .Include(a => a.User)
                .FirstOrDefault(a => a.AssessmentID == assessmentId);

            if (assessment == null)
                return NotFound("Bedömningen kunde inte hittas.");

            var items = _context.AssessmentItems
                .Where(i => i.AssessmentID == assessmentId)
                .Include(i => i.Question)
                .OrderBy(i => i.QuestionID)
                .ToList();

            if (items.All(i => !i.PatientAnswer.HasValue && !i.StaffAnswer.HasValue))
                return BadRequest("Det finns inga svar att jämföra – alla frågor är obesvarade.");

            var result = items.Select(i =>
            {
                var p = i.PatientAnswer;
                var s = i.StaffAnswer;

                string status;
                if (!p.HasValue) status = "skipped";
                else if (!s.HasValue) status = "staff-skipped";
                else if (p.Value == s.Value) status = "match";
                else if (Math.Abs(p.Value - s.Value) == 1) status = "mild-diff";
                else status = "strong-diff";

                return new StaffComparisonRowDto
                {
                    QuestionNumber = i.QuestionID,
                    QuestionText = i.Question?.QuestionText ?? "",
                    Category = i.Question?.Category ?? "",
                    PatientAnswer = p,
                    PatientComment = i.PatientComment,
                    StaffAnswer = s,
                    StaffComment = i.StaffComment,
                    Classification = status,
                    SkippedByPatient = !p.HasValue,
                    IsFlagged = i.Flag,
                    CreatedAt = assessment.CreatedAt ?? DateTime.MinValue,
                    Username = assessment.User?.Username ?? "Okänd"
                };
            }).ToList();

            return Ok(result);
        }
    }
}