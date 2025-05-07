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

    }
}
