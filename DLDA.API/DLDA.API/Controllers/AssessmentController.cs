using Microsoft.EntityFrameworkCore;
using DLDA.API.Data;
using DLDA.API.DTOs;
using DLDA.API.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AssessmentController : ControllerBase
{
    private readonly AppDbContext _context;

    public AssessmentController(AppDbContext context)
    {
        _context = context;
    }

    // Hämta alla bedömningar
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAssessments()
    {
        return await _context.Assessments
            .Select(a => new AssessmentDto
            {
                AssessmentID = a.AssessmentID,
                Type = a.Type,
                ScaleType = a.ScaleType,
                IsComplete = a.IsComplete,
                UserId = a.UserId
            }).ToListAsync();
    }

    // Hämta en specifik bedömning
    [HttpGet("{id}")]
    public async Task<ActionResult<AssessmentDto>> GetAssessment(int id)
    {
        var assessment = await _context.Assessments.FindAsync(id);
        if (assessment == null) return NotFound();

        return new AssessmentDto
        {
            AssessmentID = assessment.AssessmentID,
            Type = assessment.Type,
            ScaleType = assessment.ScaleType,
            IsComplete = assessment.IsComplete,
            UserId = assessment.UserId
        };
    }

    // Skapa en ny bedömning
    [HttpPost]
    public async Task<ActionResult> CreateAssessment(AssessmentDto dto)
    {
        var assessment = new Assessment
        {
            Type = dto.Type,
            ScaleType = dto.ScaleType,
            IsComplete = dto.IsComplete,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = dto.UserId
        };
        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAssessment), new { id = assessment.AssessmentID }, dto);
    }

    // Uppdatera en befintlig bedömning
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAssessment(int id, AssessmentDto dto)
    {
        if (id != dto.AssessmentID) return BadRequest();

        var assessment = await _context.Assessments.FindAsync(id);
        if (assessment == null) return NotFound();

        assessment.Type = dto.Type;
        assessment.ScaleType = dto.ScaleType;
        assessment.IsComplete = dto.IsComplete;
        assessment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Ta bort en bedömning
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAssessment(int id)
    {
        var assessment = await _context.Assessments.FindAsync(id);
        if (assessment == null) return NotFound();

        _context.Assessments.Remove(assessment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
