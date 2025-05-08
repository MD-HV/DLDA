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

    // --------------------------
    // [PATIENT] – Endast åtkomst till egna bedömningar
    // --------------------------

    // GET: api/Assessment/user/{userId}
    // Returnerar alla bedömningar som tillhör en specifik användare
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAssessmentsForUser(int userId)
    {
        return await _context.Assessments
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AssessmentDto
            {
                AssessmentID = a.AssessmentID,
                Type = a.Type,
                ScaleType = a.ScaleType,
                IsComplete = a.IsComplete,
                UserId = a.UserId
            }).ToListAsync();
    }

    // GET: api/Assessment/{id}
    // Returnerar en specifik bedömning – kontroll av ägarskap måste ske i frontend/backend
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

    // POST: api/Assessment
    // Skapar en ny bedömning (används av både personal och patient)
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

    // --------------------------
    // [PERSONAL] – Full åtkomst till alla bedömningar
    // --------------------------

    // GET: api/Assessment
    // Returnerar samtliga bedömningar i systemet (för personal/admin)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAssessments()
    {
        return await _context.Assessments
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AssessmentDto
            {
                AssessmentID = a.AssessmentID,
                Type = a.Type,
                ScaleType = a.ScaleType,
                IsComplete = a.IsComplete,
                UserId = a.UserId
            }).ToListAsync();
    }

    // GET: api/Assessment/search
    // Söker patienter via namn och returnerar deras bedömningar
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<object>>> SearchAssessmentsByPatientName([FromQuery] string name)
    {
        var results = await _context.Assessments
            .Include(a => a.User)
            .Where(a => a.User.Username.ToLower().Contains(name.ToLower()))
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.AssessmentID,
                a.CreatedAt,
                a.Type,
                a.ScaleType,
                a.IsComplete,
                PatientName = a.User.Username,
                UserId = a.UserId
            })
            .ToListAsync();

        if (!results.Any())
            return NotFound("Inga bedömningar hittades för angivet namn.");

        return Ok(results);
    }


    // PUT: api/Assessment/{id}
    // Uppdaterar en befintlig bedömning (endast för personal)
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

    // DELETE: api/Assessment/{id}
    // Tar bort en bedömning (endast för personal/admin)
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
