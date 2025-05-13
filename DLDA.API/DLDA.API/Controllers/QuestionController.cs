using Microsoft.EntityFrameworkCore;
using DLDA.API.Data;
using DLDA.API.DTOs;
using DLDA.API.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class QuestionController : ControllerBase
{
    private readonly AppDbContext _context;

    public QuestionController(AppDbContext context)
    {
        _context = context;
    }

    // --------------------------
    // [ADMIN] – Hantera frågedefinitioner
    // --------------------------

    // GET: api/Question
    /// <summary>Hämtar alla frågor från databasen.</summary>
    [HttpGet]
    public ActionResult<IEnumerable<QuestionDto>> GetQuestions()
    {
        return _context.Questions
            .Select(q => new QuestionDto
            {
                QuestionID = q.QuestionID,
                QuestionText = q.QuestionText ?? "",
                Category = q.Category ?? "",
                IsActive = q.IsActive
            }).ToList();
    }

    // GET: api/Question/category/{category}
    /// <summary>Hämtar alla frågor i en viss kategori.</summary>
    [HttpGet("category/{category}")]
    public ActionResult<IEnumerable<QuestionDto>> GetQuestionsByCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return BadRequest("Kategori får inte vara tom.");

        var questions = _context.Questions
            .Where(q => (q.Category ?? "").ToLower() == category.ToLower())
            .Select(q => new QuestionDto
            {
                QuestionID = q.QuestionID,
                QuestionText = q.QuestionText ?? "",
                Category = q.Category ?? "",
                IsActive = q.IsActive
            }).ToList();

        if (!questions.Any()) return NotFound($"Inga frågor hittades för kategori: {category}");

        return Ok(questions);
    }

    // GET: api/Question/5
    /// <summary>Hämtar en specifik fråga utifrån ID.</summary>
    [HttpGet("{id}")]
    public ActionResult<QuestionDto> GetQuestion(int id)
    {
        var q = _context.Questions.Find(id);
        if (q == null) return NotFound();

        return new QuestionDto
        {
            QuestionID = q.QuestionID,
            QuestionText = q.QuestionText ?? "",
            Category = q.Category ?? "",
            IsActive = q.IsActive
        };
    }

    // POST: api/Question
    /// <summary>Skapar en ny fråga.</summary>
    [HttpPost]
    public IActionResult CreateQuestion(QuestionDto dto)
    {
        var question = new Question
        {
            QuestionText = dto.QuestionText,
            Category = dto.Category,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _context.Questions.Add(question);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetQuestion), new { id = question.QuestionID }, dto);
    }

    // PUT: api/Question/5
    /// <summary>Uppdaterar en befintlig fråga.</summary>
    [HttpPut("{id}")]
    public IActionResult UpdateQuestion(int id, QuestionDto dto)
    {
        if (id != dto.QuestionID) return BadRequest();

        var question = _context.Questions.Find(id);
        if (question == null) return NotFound();

        question.QuestionText = dto.QuestionText;
        question.Category = dto.Category;
        question.IsActive = dto.IsActive;
        _context.SaveChanges();
        return NoContent();
    }

    // DELETE: api/Question/5
    /// <summary>Raderar en fråga från databasen.</summary>
    [HttpDelete("{id}")]
    public IActionResult DeleteQuestion(int id)
    {
        var question = _context.Questions.Find(id);
        if (question == null) return NotFound();

        _context.Questions.Remove(question);
        _context.SaveChanges();
        return NoContent();
    }

    // --------------------------
    // [QUIZ – PATIENT]
    // --------------------------

    // GET: api/Question/quiz/patient/next/{assessmentId}
    /// <summary>Hämtar nästa obesvarade fråga för patienten.</summary>
    // GET: api/Question/quiz/patient/next/{assessmentId}
    [HttpGet("quiz/patient/next/{assessmentId}")]
    public async Task<ActionResult<QuestionDto>> GetNextUnansweredQuestion(int assessmentId)
    {
        // 1. Försök hitta obesvarad och inte överhoppad
        var item = await _context.AssessmentItems
            .Include(ai => ai.Question)
            .Where(ai =>
                ai.AssessmentID == assessmentId &&
                (ai.PatientAnswer == null || ai.PatientAnswer == -1) &&
                !ai.SkippedByPatient)
            .OrderBy(ai => ai.Order)
            .FirstOrDefaultAsync();

        // 2. Om alla obesvarade är skippade – då är vi klara
        if (item == null)
        {
            return NotFound(new { message = "Alla frågor är besvarade eller överhoppade." });
        }

        var total = await _context.AssessmentItems.CountAsync(ai => ai.AssessmentID == assessmentId);
        var assessment = await _context.Assessments.FirstOrDefaultAsync(a => a.AssessmentID == assessmentId);

        return Ok(new QuestionDto
        {
            AssessmentID = item.AssessmentID,
            AssessmentItemID = item.ItemID, // 👈 korrekt fält som frontenden ska använda
            QuestionID = item.QuestionID,
            QuestionText = item.Question?.QuestionText ?? "Frågetext saknas",
            Category = item.Question?.Category ?? "Okänd",
            IsActive = item.Question?.IsActive ?? false,
            Order = item.Order,
            Total = total,
            ScaleType = assessment?.ScaleType ?? "Numerisk"
        });
    }



    // POST: api/Question/quiz/patient/submit
    /// <summary>Sparar patientens svar och kommentar på en fråga.</summary>
    [HttpPost("quiz/patient/submit")]
    public IActionResult SubmitPatientAnswer([FromBody] SubmitAnswerDto dto)
    {
        var item = _context.AssessmentItems.Find(dto.ItemID);
        if (item == null) return NotFound();

        item.PatientAnswer = dto.Answer;
        item.PatientComment = dto.Comment;
        item.AnsweredAt = DateTime.UtcNow;
        _context.SaveChanges();

        return Ok();
    }

    // POST: api/Question/quiz/patient/skip
    /// <summary>Markerar att patienten hoppat över en fråga.</summary>
    [HttpPost("quiz/patient/skip")]
    public IActionResult SkipPatientQuestion([FromBody] SkipQuestionDto dto)
    {
        var item = _context.AssessmentItems.Find(dto.ItemID);
        if (item == null) return NotFound();

        item.SkippedByPatient = true; // ✅ nytt
        item.AnsweredAt = DateTime.UtcNow;

        _context.SaveChanges();

        return Ok();
    }


    // GET: api/Question/quiz/patient/progress/{assessmentId}/{questionId}
    /// <summary>Visar vilken fråga patienten är på i bedömningen.</summary>
    [HttpGet("quiz/patient/progress/{assessmentId}/{questionId}")]
    public IActionResult GetPatientQuestionProgress(int assessmentId, int questionId)
    {
        var allItems = _context.AssessmentItems
            .Where(i => i.AssessmentID == assessmentId)
            .Include(i => i.Question)
            .OrderBy(i => i.QuestionID)
            .ToList();

        var index = allItems.FindIndex(i => i.QuestionID == questionId);
        if (index == -1) return NotFound();

        var item = allItems[index];

        return Ok(new
        {
            QuestionNumber = index + 1,
            TotalQuestions = allItems.Count,
            QuestionText = item.Question?.QuestionText,
            Category = item.Question?.Category
        });
    }

    // GET: api/Question/quiz/patient/previous/{assessmentId}/{currentOrder}
    [HttpGet("quiz/patient/previous/{assessmentId}/{currentOrder}")]
    public async Task<ActionResult<QuestionDto>> GetPreviousQuestion(int assessmentId, int currentOrder)
    {
        if (currentOrder <= 0)
            return NotFound("Ingen tidigare fråga.");

        var item = await _context.AssessmentItems
            .Include(i => i.Question)
            .Where(i => i.AssessmentID == assessmentId && i.Order == currentOrder - 1)
            .FirstOrDefaultAsync();

        if (item == null || item.Question == null)
            return NotFound();

        var total = await _context.AssessmentItems
            .CountAsync(i => i.AssessmentID == assessmentId);

        var assessment = await _context.Assessments
            .FirstOrDefaultAsync(a => a.AssessmentID == assessmentId);

        return Ok(new QuestionDto
        {
            ItemID = item.ItemID,
            AssessmentID = item.AssessmentID,
            QuestionID = item.Question.QuestionID,
            QuestionText = item.Question.QuestionText ?? "",
            Category = item.Question.Category ?? "",
            IsActive = item.Question.IsActive,
            Order = item.Order,
            Total = total,
            ScaleType = assessment?.ScaleType ?? "Numerisk"
        });
    }

    // --------------------------
    // [QUIZ – STAFF]
    // --------------------------

    // GET: api/Question/quiz/staff/next/{assessmentId}
    /// <summary>Hämtar nästa obesvarade fråga för personalen.</summary>
    [HttpGet("quiz/staff/next/{assessmentId}")]
    public async Task<ActionResult<StaffQuestionDto>> GetNextUnansweredStaffQuestion(int assessmentId)
    {
        Console.WriteLine($"[DEBUG] Staff next frågehämtning för assessment {assessmentId}");

        var item = await _context.AssessmentItems
            .Include(ai => ai.Question)
            .Where(ai =>
                ai.AssessmentID == assessmentId &&
                (ai.StaffAnswer == null || ai.StaffAnswer == -1))
            .OrderBy(ai => ai.Order)
            .FirstOrDefaultAsync();

        if (item == null)
        {
            Console.WriteLine("[DEBUG] Alla frågor besvarade av personalen");
            return NotFound(new { message = "Alla frågor är besvarade." });
        }

        var total = await _context.AssessmentItems.CountAsync(ai => ai.AssessmentID == assessmentId);
        var assessment = await _context.Assessments.FirstOrDefaultAsync(a => a.AssessmentID == assessmentId);

        return Ok(new StaffQuestionDto
        {
            ItemID = item.ItemID,
            AssessmentID = item.AssessmentID,
            QuestionID = item.QuestionID,
            QuestionText = item.Question?.QuestionText ?? "Frågetext saknas",
            Category = item.Question?.Category ?? "Okänd",
            Order = item.Order,
            Total = total,
            ScaleType = assessment?.ScaleType ?? "Numerisk",
            PatientAnswer = item.PatientAnswer,
            PatientComment = item.PatientComment,
            StaffAnswer = item.StaffAnswer,
            StaffComment = item.StaffComment,
            Flag = item.Flag
        });
    }


    // POST: api/Question/quiz/staff/submit
    /// <summary>Sparar personalsvar, kommentar och eventuell flagga.</summary>
    [HttpPost("quiz/staff/submit")]
    public IActionResult SubmitStaffAnswer([FromBody] SubmitStaffAnswerDto dto)
    {
        var item = _context.AssessmentItems.Find(dto.ItemID);
        if (item == null) return NotFound();

        item.StaffAnswer = dto.Answer;
        item.StaffComment = dto.Comment;
        item.Flag = dto.Flag ?? false;
        item.AnsweredAt = DateTime.UtcNow;

        // 🔽 Hämta bedömning och kontrollera om alla frågor nu är besvarade
        var assessmentId = item.AssessmentID;

        _context.SaveChanges();

        var allAnswered = _context.AssessmentItems
            .Where(i => i.AssessmentID == assessmentId)
            .All(i => i.StaffAnswer.HasValue);

        if (allAnswered)
        {
            var assessment = _context.Assessments.Find(assessmentId);
            if (assessment != null)
            {
                assessment.IsStaffComplete = true;
                assessment.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
            }
        }

        return Ok();
    }


    // POST: api/Question/quiz/staff/skip
    /// <summary>Markerar att personalen hoppat över en fråga.</summary>
    [HttpPost("quiz/staff/skip")]
    public IActionResult SkipStaffQuestion([FromBody] SkipQuestionDto dto)
    {
        var item = _context.AssessmentItems.Find(dto.ItemID);
        if (item == null) return NotFound();

        item.StaffAnswer = null;
        item.AnsweredAt = DateTime.UtcNow;
        _context.SaveChanges();

        return Ok();
    }

    // GET: api/Question/quiz/staff/progress/{assessmentId}/{questionId}
    /// <summary>Visar vilken fråga personalen är på samt patientens svar och flagga.</summary>
    [HttpGet("quiz/staff/progress/{assessmentId}/{questionId}")]
    public IActionResult GetStaffQuestionProgress(int assessmentId, int questionId)
    {
        var allItems = _context.AssessmentItems
            .Where(i => i.AssessmentID == assessmentId)
            .Include(i => i.Question)
            .OrderBy(i => i.QuestionID)
            .ToList();

        var index = allItems.FindIndex(i => i.QuestionID == questionId);
        if (index == -1) return NotFound();

        var item = allItems[index];

        return Ok(new
        {
            QuestionNumber = index + 1,
            TotalQuestions = allItems.Count,
            QuestionText = item.Question?.QuestionText,
            Category = item.Question?.Category,
            PatientAnswer = item.PatientAnswer,
            PatientComment = item.PatientComment,
            StaffComment = item.StaffComment,
            Flag = item.Flag
        });
    }
}
