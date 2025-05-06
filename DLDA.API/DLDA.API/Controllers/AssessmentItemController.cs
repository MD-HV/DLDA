using Microsoft.EntityFrameworkCore;
using DLDA.API.Data;
using DLDA.API.DTOs;
using DLDA.API.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AssessmentItemController : ControllerBase
{
    private readonly AppDbContext _context;

    public AssessmentItemController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/AssessmentItem
    // Returnerar alla bedömningsposter (frågesvar)
    [HttpGet]
    public ActionResult<IEnumerable<AssessmentItemDto>> GetItems()
    {
        return _context.AssessmentItems
            .Select(ai => new AssessmentItemDto
            {
                ItemID = ai.ItemID,
                AssessmentID = ai.AssessmentID,
                QuestionID = ai.QuestionID,
                AnswerValue = ai.AnswerValue,
                Flag = ai.Flag
            }).ToList();
    }

    // GET: api/AssessmentItem/5
    // Hämtar ett enskilt bedömningsitem
    [HttpGet("{id}")]
    public ActionResult<AssessmentItemDto> GetItem(int id)
    {
        var item = _context.AssessmentItems.Find(id);
        if (item == null) return NotFound();

        return new AssessmentItemDto
        {
            ItemID = item.ItemID,
            AssessmentID = item.AssessmentID,
            QuestionID = item.QuestionID,
            AnswerValue = item.AnswerValue,
            Flag = item.Flag
        };
    }

    // POST: api/AssessmentItem
    // Skapar en ny post (frågesvar) i en bedömning
    [HttpPost]
    public IActionResult CreateItem(AssessmentItemDto dto)
    {
        var item = new AssessmentItem
        {
            AssessmentID = dto.AssessmentID,
            QuestionID = dto.QuestionID,
            AnswerValue = dto.AnswerValue,
            Flag = dto.Flag,
            AnsweredAt = DateTime.UtcNow
        };
        _context.AssessmentItems.Add(item);
        _context.SaveChanges();
        return CreatedAtAction(nameof(GetItem), new { id = item.ItemID }, dto);
    }

    // PUT: api/AssessmentItem/5
    // Uppdaterar ett befintligt bedömningssvar
    [HttpPut("{id}")]
    public IActionResult UpdateItem(int id, AssessmentItemDto dto)
    {
        if (id != dto.ItemID) return BadRequest();

        var item = _context.AssessmentItems.Find(id);
        if (item == null) return NotFound();

        item.AnswerValue = dto.AnswerValue;
        item.Flag = dto.Flag;

        _context.SaveChanges();
        return NoContent();
    }

    // DELETE: api/AssessmentItem/5
    // Raderar ett frågesvar
    [HttpDelete("{id}")]
    public IActionResult DeleteItem(int id)
    {
        var item = _context.AssessmentItems.Find(id);
        if (item == null) return NotFound();

        _context.AssessmentItems.Remove(item);
        _context.SaveChanges();
        return NoContent();
    }
}
