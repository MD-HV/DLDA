using DLDA.API.Data;
using DLDA.API.DTOs;
using DLDA.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

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
    // Returnerar alla bedömningar som tillhör en specifik användare + om den påbörjats
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAssessmentsForUser(int userId)
    {
        return await _context.Assessments
            .Where(a => a.UserId == userId)
            .Include(a => a.AssessmentItems) // 👈 Behövs för att kunna kontrollera svar
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AssessmentDto
            {
                AssessmentID = a.AssessmentID,
                ScaleType = a.ScaleType,
                IsComplete = a.IsComplete,
                IsStaffComplete = a.IsStaffComplete,
                UserId = a.UserId,
                CreatedAt = a.CreatedAt ?? DateTime.MinValue,
                HasStarted = a.AssessmentItems.Any(i => i.PatientAnswer != null || i.SkippedByPatient),
                AnsweredCount = a.AssessmentItems.Count(i => i.PatientAnswer != null || i.SkippedByPatient),
                TotalQuestions = a.AssessmentItems.Count
            })
            .ToListAsync();
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
            ScaleType = assessment.ScaleType,
            IsComplete = assessment.IsComplete,
            UserId = assessment.UserId,
            CreatedAt = assessment.CreatedAt ?? DateTime.MinValue
        };
    }

    // --------------------------
    // [PERSONAL] – Full åtkomst till alla bedömningar
    // --------------------------

    // POST: api/Assessment
    // Skapar en ny bedömning och kopplar alla aktiva frågor som AssessmentItems
    [HttpPost]
    public async Task<ActionResult<AssessmentDto>> CreateAssessment(AssessmentDto dto)
    {
        try
        {
            Console.WriteLine($"[INFO] Skapar ny assessment för UserId={dto.UserId}");

            var assessment = new Assessment
            {
                ScaleType = dto.ScaleType,
                IsComplete = dto.IsComplete,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = dto.UserId
            };

            _context.Assessments.Add(assessment);
            await _context.SaveChangesAsync(); // ✅ Här skapas ID:t

            Console.WriteLine($"[INFO] Assessment sparad med ID={assessment.AssessmentID}");

            var questions = await _context.Questions
                .Where(q => q.IsActive)
                .OrderBy(q => q.QuestionID)
                .ToListAsync();

            Console.WriteLine($"[INFO] Antal aktiva frågor hämtade: {questions.Count}");

            if (!questions.Any())
            {
                Console.WriteLine("[WARN] Inga aktiva frågor hittades att koppla till bedömningen.");
                return BadRequest("Inga aktiva frågor hittades att koppla till bedömningen.");
            }

            int index = 0;
            foreach (var question in questions)
            {
                var item = new AssessmentItem
                {
                    AssessmentID = assessment.AssessmentID,
                    QuestionID = question.QuestionID,
                    PatientAnswer = null,
                    StaffAnswer = null,
                    Flag = false,
                    SkippedByPatient = false, // ✅ Nytt fält
                    AnsweredAt = null,
                    Order = index++
                };
                _context.AssessmentItems.Add(item);
            }

            Console.WriteLine($"[INFO] Totalt {index} AssessmentItems skapades. Försöker spara...");

            await _context.SaveChangesAsync();

            Console.WriteLine("[SUCCESS] AssessmentItems sparades korrekt.");

            return CreatedAtAction(nameof(GetAssessment), new { id = assessment.AssessmentID }, new AssessmentDto
            {
                AssessmentID = assessment.AssessmentID,
                ScaleType = assessment.ScaleType,
                IsComplete = assessment.IsComplete,
                UserId = assessment.UserId,
                CreatedAt = assessment.CreatedAt ?? DateTime.MinValue
            });
        }
        catch (DbUpdateException dbEx)
        {
            Console.WriteLine($"[DB ERROR] {dbEx.InnerException?.Message ?? dbEx.Message}");
            return StatusCode(500, "Databasfel vid sparande av bedömning.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            return StatusCode(500, "Ett internt fel uppstod vid skapande av bedömning.");
        }
    }

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
                ScaleType = a.ScaleType,
                IsComplete = a.IsComplete,
                UserId = a.UserId,
                CreatedAt = a.CreatedAt ?? DateTime.MinValue
            }).ToListAsync();
    }

    // GET: api/Assessment/search
    // Söker patienter via namn och returnerar deras bedömningar
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<object>>> SearchAssessmentsByPatientName([FromQuery] string name)
    {
        var results = await _context.Assessments
            .Include(a => a.User)
            .Where(a => a.User != null && a.User.Username.ToLower().Contains(name.ToLower()))
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.AssessmentID,
                CreatedAt = a.CreatedAt ?? DateTime.MinValue,
                a.ScaleType,
                a.IsComplete,
                PatientName = a.User!.Username,
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

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Om raden redan är borta, behandla det som "OK"
            return NoContent(); // eller: return NotFound();
        }

        return NoContent();
    }

    // POST: /StaffAssessment/Unlock
    // låser upp en redan avklarad bedömning
    [HttpPost("unlock/{assessmentId}")]
    public IActionResult UnlockAssessment(int assessmentId)
    {
        var assessment = _context.Assessments.FirstOrDefault(a => a.AssessmentID == assessmentId);
        if (assessment == null)
            return NotFound();

        assessment.IsStaffComplete = false;
        _context.SaveChanges();

        return Ok();
    }


}
