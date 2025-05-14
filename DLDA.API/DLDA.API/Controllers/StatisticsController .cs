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

        // GET: api/statistics/progress-feedback/{userId}
        // Returnerar positiv återkoppling till patienten baserat på förbättring sen senaste test
        [HttpGet("progress-feedback/{userId}")]
        public ActionResult<object> GetProgressFeedback(int userId)
        {
            var assessments = _context.Assessments
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(2)
                .ToList();

            if (!assessments.Any())
                return NotFound("Inga bedömningar hittades för användaren.");

            var latest = assessments.First();
            var latestItems = _context.AssessmentItems
                .Where(i => i.AssessmentID == latest.AssessmentID && i.PatientAnswer.HasValue)
                .Include(i => i.Question)
                .ToList();

            if (assessments.Count == 1)
            {
                var strengths = latestItems
                    .Where(i => i.PatientAnswer!.Value <= 1)
                    .Select(i => i.Question!.QuestionText)
                    .ToList();

                return Ok(new
                {
                    Message = "Följande områden skattades som fungerande utan större svårigheter:",
                    Questions = strengths
                });
            }

            var previous = assessments[1];
            var previousItems = _context.AssessmentItems
                .Where(i => i.AssessmentID == previous.AssessmentID && i.PatientAnswer.HasValue)
                .ToDictionary(i => i.QuestionID, i => i.PatientAnswer!.Value);

            var minorImprovements = new List<object>();
            var clearImprovements = new List<object>();

            foreach (var item in latestItems)
            {
                if (!previousItems.ContainsKey(item.QuestionID)) continue;

                var current = item.PatientAnswer!.Value;
                var previousValue = previousItems[item.QuestionID];
                var diff = previousValue - current;

                if (diff == 1)
                {
                    minorImprovements.Add(new
                    {
                        Question = item.Question?.QuestionText ?? "(okänd fråga)",
                        Previous = previousValue,
                        Current = current
                    });
                }
                else if (diff >= 2)
                {
                    clearImprovements.Add(new
                    {
                        Question = item.Question?.QuestionText ?? "(okänd fråga)",
                        Previous = previousValue,
                        Current = current
                    });
                }
            }

            return Ok(new
            {
                Message = (minorImprovements.Any() || clearImprovements.Any())
                    ? "Jämförelsen visar förbättring i vissa områden:"
                    : "Inga tydliga förbättringar jämfört med föregående bedömning.",
                TydligFörbättring = clearImprovements,
                LitenFörbättring = minorImprovements
            });
        }

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
        // Returnerar summering av patientens senaste bedömning (enbart patientens svar)
        [HttpGet("summary/{assessmentId}")]
        public ActionResult<object> GetSingleAssessmentSummary(int assessmentId)
        {
            var items = _context.AssessmentItems
                .Where(i => i.AssessmentID == assessmentId && i.PatientAnswer.HasValue)
                .ToList();

            if (!items.Any())
                return NotFound("Inga besvarade frågor hittades.");

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
                    .Where(a => a.AssessmentID == assessmentId)
                    .FirstOrDefault();

                if (assessment == null)
                    return NotFound("Bedömningen kunde inte hittas.");

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
                Console.WriteLine($"[ERROR] Fel i GetSingleSummary: {ex.Message}");
                return StatusCode(500, "Ett internt fel uppstod vid hämtning av statistik.");
            }
        }




        // --------------------------
        // [PERSONAL] – Fördjupad analys och jämförelse
        // --------------------------

        // GET: api/statistics/match/{assessmentId}
        // Returnerar matchningsstatistik mellan patient och personal
        [HttpGet("match/{assessmentId}")]
        public ActionResult<object> GetMatchStatistics(int assessmentId)
        {
            var items = _context.AssessmentItems
                .Where(i => i.AssessmentID == assessmentId &&
                            i.PatientAnswer.HasValue && i.StaffAnswer.HasValue)
                .ToList();

            if (!items.Any())
                return NotFound("Inga jämförbara svar hittades.");

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

        // GET: api/statistics/total/{assessmentId}
        // Returnerar totalt antal frågor i en bedömning
        [HttpGet("total/{assessmentId}")]
        public ActionResult<int> GetTotalQuestions(int assessmentId)
        {
            int total = _context.AssessmentItems
                .Count(i => i.AssessmentID == assessmentId);

            return Ok(total);
        }

        // GET: api/statistics/skipped/{assessmentId}
        // Returnerar alla obesvarade frågor för en bedömning (används internt)
        [HttpGet("skipped/{assessmentId}")]
        public ActionResult<IEnumerable<string>> GetSkippedQuestions(int assessmentId)
        {
            var skipped = _context.AssessmentItems
                .Where(i => i.AssessmentID == assessmentId && !i.PatientAnswer.HasValue)
                .Include(i => i.Question)
                .Select(i => i.Question != null ? i.Question.QuestionText : "[Fråga saknas]")
                .ToList();

            return Ok(skipped);
        }

        // GET: api/statistics/comparison-table/{assessmentId}
        // Returnerar detaljerad rad-för-rad jämförelse av svar (patient & personal)
        [HttpGet("comparison-table/{assessmentId}")]
        public ActionResult<IEnumerable<object>> GetAssessmentComparisonTable(int assessmentId)
        {
            var items = _context.AssessmentItems
                .Where(i => i.AssessmentID == assessmentId)
                .Include(i => i.Question)
                .OrderBy(i => i.QuestionID)
                .ToList();

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

                return new
                {
                    QuestionNumber = i.QuestionID,
                    QuestionText = i.Question?.QuestionText ?? "",
                    PatientAnswer = p.HasValue ? p.Value.ToString() : "(skippad)",
                    StaffAnswer = s.HasValue ? s.Value.ToString() : "(skippad)",
                    Classification = status,
                    Comment = i.Flag ? "Diskutera vidare ⚠️" : ""
                };
            });

            return Ok(result);
        }

        // GET: api/statistics/patient-change-overview/{userId}
        // Returnerar förändringar mellan två senaste bedömningar, uppdelat i tre grupper
        [HttpGet("patient-change-overview/{userId}")]
        public ActionResult<object> GetPatientChangeOverviewGrouped(int userId)
        {
            var assessments = _context.Assessments
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(2)
                .ToList();

            if (assessments.Count < 2)
                return Ok("Det finns inte tillräckligt många bedömningar för att visa förändringar.");

            var latest = assessments[0];
            var previousAssessment = assessments[1];

            var latestItems = _context.AssessmentItems
                .Where(i => i.AssessmentID == latest.AssessmentID && i.PatientAnswer.HasValue)
                .Include(i => i.Question)
                .ToList();

            var previousItems = _context.AssessmentItems
                .Where(i => i.AssessmentID == previousAssessment.AssessmentID && i.PatientAnswer.HasValue)
                .ToDictionary(i => i.QuestionID, i => i.PatientAnswer!.Value);

            var improvements = new List<object>();
            var deteriorations = new List<object>();
            var unchanged = new List<object>();

            foreach (var item in latestItems)
            {
                if (!previousItems.ContainsKey(item.QuestionID)) continue;

                var previousValue = previousItems[item.QuestionID];
                var currentValue = item.PatientAnswer!.Value;
                var questionText = item.Question?.QuestionText ?? "(okänd fråga)";
                var changeAmount = Math.Abs(currentValue - previousValue);

                if (currentValue < previousValue)
                {
                    improvements.Add(new { Question = questionText, Previous = previousValue, Current = currentValue, Change = changeAmount });
                }
                else if (currentValue > previousValue)
                {
                    deteriorations.Add(new { Question = questionText, Previous = previousValue, Current = currentValue, Change = changeAmount });
                }
                else
                {
                    unchanged.Add(new { Question = questionText, Value = currentValue });
                }
            }

            var sortedImprovements = improvements.OrderByDescending(i => ((dynamic)i).Change).ToList();
            var sortedDeteriorations = deteriorations.OrderByDescending(i => ((dynamic)i).Change).ToList();

            return Ok(new
            {
                Förbättringar = sortedImprovements,
                Försämringar = sortedDeteriorations,
                Oförändrat = unchanged
            });
        }
    }
}
