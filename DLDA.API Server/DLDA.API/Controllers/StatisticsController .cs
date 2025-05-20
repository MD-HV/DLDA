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

        [HttpGet("compare-assessments/{firstId}/{secondId}")]
        public ActionResult<PatientChangeOverviewDto> CompareAssessments(int firstId, int secondId)
        {
            var a1 = _context.Assessments
                .Include(a => a.AssessmentItems)
                .ThenInclude(i => i.Question)
                .FirstOrDefault(a => a.AssessmentID == firstId);

            var a2 = _context.Assessments
                .Include(a => a.AssessmentItems)
                .ThenInclude(i => i.Question)
                .FirstOrDefault(a => a.AssessmentID == secondId);

            if (a1 == null || a2 == null)
                return NotFound("En eller båda bedömningarna kunde inte hittas.");

            if (a1.AssessmentItems.All(i => !i.PatientAnswer.HasValue) ||
                a2.AssessmentItems.All(i => !i.PatientAnswer.HasValue))
            {
                return BadRequest("En eller båda bedömningarna saknar besvarade frågor.");
            }

            var förbättringar = new List<ImprovementApiDto>();

            foreach (var item1 in a1.AssessmentItems)
            {
                var item2 = a2.AssessmentItems.FirstOrDefault(i => i.QuestionID == item1.QuestionID);
                if (item1.PatientAnswer.HasValue && item2?.PatientAnswer.HasValue == true)
                {
                    var dto = new ImprovementApiDto
                    {
                        QuestionID = item1.QuestionID,
                        Question = item1.Question?.QuestionText ?? "[Okänd fråga]",
                        Category = item1.Question?.Category ?? "",
                        Previous = item1.PatientAnswer.Value,
                        Current = item2.PatientAnswer.Value,
                        SkippedPrevious = false,
                        SkippedCurrent = false
                    };

                    förbättringar.Add(dto);
                }
            }

            return Ok(new PatientChangeOverviewDto
            {
                PreviousDate = a1.CreatedAt ?? DateTime.MinValue,
                CurrentDate = a2.CreatedAt ?? DateTime.MinValue,
                Förbättringar = förbättringar.Where(f => f.Change >= 1).ToList()
            });
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

        /// <summary>
        /// Jämför två personalbedömningar och returnerar förbättringar, försämringar, flaggade och obesvarade frågor.
        /// </summary>
        [HttpGet("staff-compare-assessments/{firstId}/{secondId}")]
        public ActionResult<StaffChangeOverviewDto> CompareStaffAssessments(int firstId, int secondId)
        {
            var a1 = _context.Assessments
                .Include(a => a.User)
                .Include(a => a.AssessmentItems)
                .ThenInclude(i => i.Question)
                .FirstOrDefault(a => a.AssessmentID == firstId);

            var a2 = _context.Assessments
                .Include(a => a.User)
                .Include(a => a.AssessmentItems)
                .ThenInclude(i => i.Question)
                .FirstOrDefault(a => a.AssessmentID == secondId);

            if (a1 == null || a2 == null)
                return NotFound("En eller båda bedömningarna kunde inte hittas.");

            if (a1.AssessmentItems.All(i => !i.StaffAnswer.HasValue) ||
                a2.AssessmentItems.All(i => !i.StaffAnswer.HasValue))
            {
                return BadRequest("En eller båda bedömningarna saknar personalsvar.");
            }

            var förbättringar = new List<ImprovementDto>();
            var försämringar = new List<ImprovementDto>();
            var flaggade = new List<ImprovementDto>();
            var hoppade = new List<ImprovementDto>();

            foreach (var item1 in a1.AssessmentItems)
            {
                var item2 = a2.AssessmentItems.FirstOrDefault(i => i.QuestionID == item1.QuestionID);
                if (item2 == null || item1.Question == null) continue;

                var question = item1.Question; 

                var prev = item1.StaffAnswer;
                var curr = item2.StaffAnswer;

                var dto = new ImprovementDto
                {
                    QuestionId = item1.QuestionID,
                    Question = question?.QuestionText ?? "[Okänd fråga]",
                    Category = question?.Category ?? "[Okänd kategori]",
                    Previous = prev ?? -1,
                    Current = curr ?? -1,
                    SkippedPrevious = !prev.HasValue,
                    SkippedCurrent = !curr.HasValue
                };

                if (!prev.HasValue || !curr.HasValue)
                {
                    hoppade.Add(dto);
                }
                else if (curr < prev)
                {
                    förbättringar.Add(dto);
                }
                else if (curr > prev)
                {
                    försämringar.Add(dto);
                }

                if (item2.Flag)
                {
                    flaggade.Add(dto);
                }
            }

            return Ok(new StaffChangeOverviewDto
            {
                Username = a1.User?.Username ?? "Okänd",
                PreviousDate = a1.CreatedAt ?? DateTime.MinValue,
                CurrentDate = a2.CreatedAt ?? DateTime.MinValue,
                Förbättringar = förbättringar,
                Försämringar = försämringar,
                Flaggade = flaggade,
                Hoppade = hoppade
            });
        }

    }
}