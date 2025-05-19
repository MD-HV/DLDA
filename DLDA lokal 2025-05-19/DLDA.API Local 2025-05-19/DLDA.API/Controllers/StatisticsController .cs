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

        // GET: api/statistics/patient-change-overview/{userId}
        // Returnerar förändringar mellan två senaste bedömningar, uppdelat i tre grupper
        [HttpGet("patient-change-overview/{userId}")]
        public ActionResult<PatientChangeOverviewDto> GetPatientChangeOverviewGrouped(int userId)
        {
            var assessments = _context.Assessments
                .Where(a => a.UserId == userId && a.IsComplete)
                .OrderByDescending(a => a.CreatedAt)
                .Take(2)
                .ToList();

            if (assessments.Count < 2)
                return Ok("Det finns inte tillräckligt många bedömningar för att visa förändringar.");

            var latest = assessments[0];
            var previousAssessment = assessments[1];

            var latestItems = _context.AssessmentItems
                .Where(i => i.AssessmentID == latest.AssessmentID)
                .Include(i => i.Question)
                .ToList();

            var previousItems = _context.AssessmentItems
                .Where(i => i.AssessmentID == previousAssessment.AssessmentID)
                .ToDictionary(i => i.QuestionID, i => i);

            var improvements = new List<ImprovementApiDto>();

            foreach (var item in latestItems)
            {
                if (!previousItems.ContainsKey(item.QuestionID)) continue;

                var previousItem = previousItems[item.QuestionID];
                if (!item.PatientAnswer.HasValue || !previousItem.PatientAnswer.HasValue) continue;

                var previousValue = previousItem.PatientAnswer.Value;
                var currentValue = item.PatientAnswer.Value;

                if (currentValue < previousValue)
                {
                    improvements.Add(new ImprovementApiDto
                    {
                        QuestionID = item.QuestionID, // 👈 Lägg till fråge-ID
                        Question = item.Question?.QuestionText ?? "(okänd fråga)",
                        Previous = previousValue,
                        Current = currentValue,
                        Category = item.Question?.Category ?? "",
                        SkippedPrevious = !previousItem.PatientAnswer.HasValue,
                        SkippedCurrent = !item.PatientAnswer.HasValue
                    });
                }
            }

            var sortedImprovements = improvements.OrderByDescending(i => i.Change).ToList();

            var result = new PatientChangeOverviewDto
            {
                PreviousDate = previousAssessment.CreatedAt ?? DateTime.MinValue,
                CurrentDate = latest.CreatedAt ?? DateTime.MinValue,
                Förbättringar = sortedImprovements.Select(i => new ImprovementDto
                {
                    QuestionId = i.QuestionID,
                    Question = i.Question,
                    Previous = i.Previous,
                    Current = i.Current,
                    Category = i.Category,
                    SkippedPrevious = i.SkippedPrevious,
                    SkippedCurrent = i.SkippedCurrent
                }).ToList()
            };

            return Ok(result);
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

        // GET: api/statistics/comparison-table-staff/{assessmentId}
        // Returnerar detaljerad rad-för-rad jämförelse av svar (patient & personal)
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

                    // 🔹 Nytt: lägg till datum och användarnamn
                    CreatedAt = assessment.CreatedAt ?? DateTime.MinValue,
                    Username = assessment.User?.Username ?? "Okänd"
                };
            }).ToList();

            return Ok(result);
        }



        // GET: api/statistics/staff-change-overview/{userId}
        [HttpGet("staff-change-overview/{userId}")]
        public ActionResult<StaffChangeOverviewDto> GetStaffChangeOverview(int userId)
        {
            var assessments = _context.Assessments
                .Where(a => a.UserId == userId && a.IsComplete)
                .Include(a => a.User) // 👈 behövs för Username
                .OrderByDescending(a => a.CreatedAt)
                .Take(2)
                .ToList();

            if (assessments.Count < 2)
                return Ok("Det finns inte tillräckligt många bedömningar för att visa förändringar.");

            var latest = assessments[0];
            var previous = assessments[1];

            var latestItems = _context.AssessmentItems
                .Where(i => i.AssessmentID == latest.AssessmentID)
                .Include(i => i.Question)
                .ToList();

            var previousItems = _context.AssessmentItems
                .Where(i => i.AssessmentID == previous.AssessmentID)
                .ToDictionary(i => i.QuestionID, i => i);

            var improvements = new List<ImprovementDto>();
            var deteriorations = new List<ImprovementDto>();
            var flags = new List<ImprovementDto>();
            var skipped = new List<ImprovementDto>();

            foreach (var item in latestItems)
            {
                if (!previousItems.ContainsKey(item.QuestionID)) continue;

                var previousItem = previousItems[item.QuestionID];

                if (!item.PatientAnswer.HasValue || !previousItem.PatientAnswer.HasValue)
                {
                    skipped.Add(new ImprovementDto
                    {
                        QuestionId = item.QuestionID,
                        Question = item.Question?.QuestionText ?? "(okänd)",
                        Category = item.Question?.Category ?? "",
                        Previous = previousItem.PatientAnswer ?? -1,
                        Current = item.PatientAnswer ?? -1,
                        SkippedPrevious = !previousItem.PatientAnswer.HasValue,
                        SkippedCurrent = !item.PatientAnswer.HasValue
                    });
                    continue;
                }

                var current = item.PatientAnswer.Value;
                var previousVal = previousItem.PatientAnswer.Value;

                var dto = new ImprovementDto
                {
                    QuestionId = item.QuestionID,
                    Question = item.Question?.QuestionText ?? "(okänd)",
                    Category = item.Question?.Category ?? "",
                    Previous = previousVal,
                    Current = current,
                    SkippedPrevious = false,
                    SkippedCurrent = false
                };

                if (current < previousVal)
                    improvements.Add(dto);
                else if (current > previousVal)
                    deteriorations.Add(dto);

                if (item.Flag)
                    flags.Add(dto);
            }

            return Ok(new StaffChangeOverviewDto
            {
                Username = latest.User?.Username ?? "Okänd",
                PreviousDate = previous.CreatedAt ?? DateTime.MinValue,
                CurrentDate = latest.CreatedAt ?? DateTime.MinValue,
                Förbättringar = improvements.OrderByDescending(i => i.Change).ToList(),
                Försämringar = deteriorations.OrderByDescending(i => i.Change).ToList(),
                Flaggade = flags,
                Hoppade = skipped
            });
        }
    }
}
