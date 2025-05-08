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

    // GET: api/AssessmentItem/patient/assessment/{assessmentId}
    // Returnerar enbart patientens svar för en viss bedömning
    [HttpGet("patient/assessment/{assessmentId}")]
    public ActionResult<IEnumerable<object>> GetPatientAnswers(int assessmentId)
    {
    var items = _context.AssessmentItems
        .Where(ai => ai.AssessmentID == assessmentId)
        .Include(ai => ai.Question)
        .OrderBy(ai => ai.QuestionID)
        .Select(ai => new
        {
            ai.ItemID,
            ai.AssessmentID,
            ai.QuestionID,
            QuestionText = ai.Question != null ? ai.Question.QuestionText : "",
            PatientAnswer = ai.PatientAnswer,
            Flag = ai.Flag
        })
        .ToList();

    return Ok(items);
    }
    
    // GET: api/AssessmentItem
    // Returnerar alla bedömningsposter med både patient- och personalsvar
    [HttpGet]
    public ActionResult<IEnumerable<AssessmentItemDto>> GetItems()
    {
        return _context.AssessmentItems
            .Select(ai => new AssessmentItemDto
            {
                ItemID = ai.ItemID,
                AssessmentID = ai.AssessmentID,
                QuestionID = ai.QuestionID,
                PatientAnswer = ai.PatientAnswer,
                StaffAnswer = ai.StaffAnswer,
                Flag = ai.Flag
            }).ToList();
    }

    // GET: api/AssessmentItem/5
    // Hämtar ett specifikt bedömningsitem
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
            PatientAnswer = item.PatientAnswer,
            StaffAnswer = item.StaffAnswer,
            Flag = item.Flag
        };
    }

    // POST: api/AssessmentItem
    // Skapar ett nytt bedömningsitem (kan innehålla antingen eller båda svaren)
    [HttpPost]
    public IActionResult CreateItem(AssessmentItemDto dto)
    {
        var item = new AssessmentItem
        {
            AssessmentID = dto.AssessmentID,
            QuestionID = dto.QuestionID,
            PatientAnswer = dto.PatientAnswer ?? -1,
            StaffAnswer = dto.StaffAnswer,
            Flag = dto.Flag,
            AnsweredAt = DateTime.UtcNow
        };

        _context.AssessmentItems.Add(item);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetItem), new { id = item.ItemID }, dto);
    }

    // PUT: api/AssessmentItem/patient/5
    // Uppdaterar en patients svar
    [HttpPut("patient/{id}")]
    public IActionResult UpdatePatientAnswer(int id, [FromBody] int answer)
    {
        var item = _context.AssessmentItems.Find(id);
        if (item == null) return NotFound();

        item.PatientAnswer = answer;
        item.AnsweredAt = DateTime.UtcNow;
        _context.SaveChanges();

        return NoContent();
    }

    // PUT: api/AssessmentItem/staff/5
    // Uppdaterar en personals svar
    [HttpPut("staff/{id}")]
    public IActionResult UpdateStaffAnswer(int id, [FromBody] int answer)
    {
        var item = _context.AssessmentItems.Find(id);
        if (item == null) return NotFound();

        item.StaffAnswer = answer;
        item.AnsweredAt = DateTime.UtcNow;
        _context.SaveChanges();

        return NoContent();
    }

    // DELETE: api/AssessmentItem/5
    // Raderar ett bedömningsitem
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
