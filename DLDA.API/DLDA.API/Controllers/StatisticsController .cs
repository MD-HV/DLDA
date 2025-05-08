using DLDA.API.Data;
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

        // GET: api/statistics/match/{assessmentId}
        // Returnerar matchningsstatistik för en specifik bedömning.
        // Endast frågor där både patient och personal har svarat inkluderas i beräkningen.
        [HttpGet("match/{assessmentId}")]
        public ActionResult<object> GetMatchStatistics(int assessmentId)
        {
            // Hämta poster där båda parter har svarat
            var items = _context.AssessmentItems
                .Where(i => i.AssessmentID == assessmentId &&
                            i.PatientAnswer.HasValue && i.StaffAnswer.HasValue)
                .ToList();

            if (!items.Any())
                return NotFound("Inga jämförbara svar hittades.");

            int matchCount = items.Count(i => i.PatientAnswer == i.StaffAnswer);
            int total = items.Count;

            // Returnera matchningsprocent
            return Ok(new
            {
                QuestionsCompared = total,
                Matches = matchCount,
                MatchPercent = Math.Round((double)matchCount / total * 100, 1),
                MismatchPercent = Math.Round(100 - ((double)matchCount / total * 100), 1)
            });
        }

        // GET: api/statistics/skipped/patient/{assessmentId}
        // Returnerar alla frågetexter i en bedömning där patienten inte har lämnat ett svar.
        [HttpGet("skipped/patient/{assessmentId}")]
        public ActionResult<object> GetSkippedByPatient(int assessmentId)
        {
            // Hämta poster där patienten inte svarat
            var skipped = _context.AssessmentItems
                .Where(i => i.AssessmentID == assessmentId && !i.PatientAnswer.HasValue)
                .Include(i => i.Question)
                .ToList();

            // Returnera antal samt listan med frågetexter (om frågan finns)
            return Ok(new
            {
                SkippedCount = skipped.Count,
                Questions = skipped
                    .Where(i => i.Question != null)
                    .Select(i => i.Question!.QuestionText)
                    .ToList()
            });
        }

        // GET: api/statistics/total/{assessmentId}
        // Returnerar totalt antal frågor som är kopplade till en specifik bedömning.
        [HttpGet("total/{assessmentId}")]
        public ActionResult<int> GetTotalQuestions(int assessmentId)
        {
            // Räkna alla items som tillhör denna bedömning
            int total = _context.AssessmentItems
                .Count(i => i.AssessmentID == assessmentId);

            return Ok(total);
        }



        // GET: api/statistics/skipped/5
        // Returnerar en lista med frågetexter som patienten inte har besvarat (PatientAnswer = null).
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
        // Returnerar en tabellrad per fråga med båda svaren, färgklassificering och ev. kommentar
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

                if (!p.HasValue)              // Går att skapa Färglogik i front end
                    status = "skipped";       // "skipped" → 🔶 orange (för patient)
                else if (!s.HasValue)
                    status = "staff-skipped"; //"staff-skipped" → valfri (t.ex. grå)
                else if (p.Value == s.Value)
                    status = "match";         // "match" → 🟩 grön
                else if (Math.Abs(p.Value - s.Value) == 1)
                    status = "mild-diff";    //   "mild-diff" → 🟨 gul
                else
                    status = "strong-diff"; // "strong-diff" → 🟥 röd

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

        // GET: api/statistics/progress-feedback/{userId}
        // Returnerar positiv återkoppling till Patient baserat på förbättring sedan senaste test
        [HttpGet("progress-feedback/{userId}")]
        public ActionResult<object> GetProgressFeedback(int userId)
        {
            // Hämta alla bedömningar för användaren, sorterat nyast först
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

            // Om det bara finns en bedömning: visa styrkor
            if (assessments.Count == 1)
            {
                var strengths = latestItems
                    .Where(i => i.PatientAnswer!.Value <= 1)
                    .Select(i => i.Question!.QuestionText)
                    .ToList();

                return Ok(new
                {
                    Message = "Dina styrkor i den här bedömningen:",
                    Questions = strengths
                });
            }

            // Jämför med föregående bedömning
            var previous = assessments[1];
            var previousItems = _context.AssessmentItems
                .Where(i => i.AssessmentID == previous.AssessmentID && i.PatientAnswer.HasValue)
                .ToDictionary(i => i.QuestionID, i => i.PatientAnswer!.Value);

            var improvements = latestItems
                .Where(i => previousItems.ContainsKey(i.QuestionID) &&
                            i.PatientAnswer < previousItems[i.QuestionID])
                .Select(i => new
                {
                    Question = i.Question!.QuestionText,
                    Previous = previousItems[i.QuestionID],
                    Current = i.PatientAnswer!.Value
                })
                .ToList();

            return Ok(new
            {
                Message = improvements.Any()
                    ? "Skillnader jämfört med föregående bedömning visar förbättring i följande områden:"
                    : "Skattningarna är oförändrade jämfört med föregående bedömning.",
                Improvements = improvements
            });
        }

        // GET: api/statistics/patient-change-overview/{userId}
        // Returnerar förändringar mellan två senaste bedömningar, uppdelat i tre kategorier
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

            // Sortera förbättringar och försämringar efter störst förändring först
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
