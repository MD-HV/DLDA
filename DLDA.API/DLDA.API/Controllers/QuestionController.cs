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

    // GET: api/Question
    // Returnerar alla frågor i frågebanken
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
    // Hämta alla frågor som tillhör en viss kategori (t.ex. "1. Lärande och att tillämpa kunskap")
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
    // Hämtar en specifik fråga
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
    // Uppdaterar en existerande fråga
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
}
