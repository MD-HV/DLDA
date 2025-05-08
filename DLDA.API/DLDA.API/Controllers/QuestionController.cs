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
    // Returnerar alla frågor i databasen
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
    // Returnerar alla frågor i vald kategori
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
    // Hämtar en fråga via ID
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
    // Skapar en ny fråga
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
    // Uppdaterar en befintlig fråga
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
    // Tar bort en fråga från databasen
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
    // Hämtar nästa obesvarade fråga för patienten
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
    // Skickar in patientens svar
    [HttpPost("quiz/patient/submit")]
    public IActionResult SubmitPatientAnswer([FromBody] SubmitAnswerDto dto)
    {
        var item = _context.AssessmentItems.Find(dto.ItemID);
        if (item == null) return NotFound();

        item.PatientAnswer = dto.Answer;
        item.AnsweredAt = DateTime.UtcNow;
        _context.SaveChanges();

        return Ok();
    }

    // POST: api/Question/quiz/patient/skip
    // Markerar att patienten hoppat över frågan
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
    // Visar fråga X av Y
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
    // Hämtar nästa obesvarade fråga för personalen
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
            Flag = nextItem.Flag
        });
    }

    // POST: api/Question/quiz/staff/submit
    // Skickar in personalsvar + flagga
    [HttpPost("quiz/staff/submit")]
    public IActionResult SubmitStaffAnswer([FromBody] SubmitStaffAnswerDto dto)
    {
        var item = _context.AssessmentItems.Find(dto.ItemID);
        if (item == null) return NotFound();

        item.StaffAnswer = dto.Answer;
        item.AnsweredAt = DateTime.UtcNow;
        item.Flag = dto.Flag ?? false;
        _context.SaveChanges();

        return Ok();
    }

    // POST: api/Question/quiz/staff/skip
    // Personal markerar fråga som överhoppad
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
    // Visar fråga X av Y samt patientens svar
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
            Flag = item.Flag
        });
    }
}
