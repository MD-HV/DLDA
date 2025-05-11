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
    [HttpGet("quiz/patient/next/{assessmentId}")]
    public IActionResult GetNextUnansweredPatientQuestion(int assessmentId)
    {
        var nextItem = _context.AssessmentItems
            .Include(i => i.Question)
            .Where(i => i.AssessmentID == assessmentId && !i.PatientAnswer.HasValue)
            .OrderBy(i => i.QuestionID)
            .FirstOrDefault();

        if (nextItem == null)
            return Ok(new { Done = true });

        return Ok(new
        {
            QuestionID = nextItem.QuestionID,
            QuestionText = nextItem.Question?.QuestionText,
            Category = nextItem.Question?.Category,
            ItemID = nextItem.ItemID
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

        item.PatientAnswer = null;
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

    // --------------------------
    // [QUIZ – PERSONAL]
    // --------------------------

    // GET: api/Question/quiz/staff/next/{assessmentId}
    /// <summary>Hämtar nästa obesvarade fråga för personalen.</summary>
    [HttpGet("quiz/staff/next/{assessmentId}")]
    public IActionResult GetNextUnansweredStaffQuestion(int assessmentId)
    {
        var nextItem = _context.AssessmentItems
            .Include(i => i.Question)
            .Where(i => i.AssessmentID == assessmentId && !i.StaffAnswer.HasValue)
            .OrderBy(i => i.QuestionID)
            .FirstOrDefault();

        if (nextItem == null)
            return Ok(new { Done = true });

        return Ok(new
        {
            QuestionID = nextItem.QuestionID,
            QuestionText = nextItem.Question?.QuestionText,
            Category = nextItem.Question?.Category,
            ItemID = nextItem.ItemID,
            PatientAnswer = nextItem.PatientAnswer,
            PatientComment = nextItem.PatientComment,
            Flag = nextItem.Flag
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

        _context.SaveChanges();
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
