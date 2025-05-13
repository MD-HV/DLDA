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

    // ----------------------
    // [PATIENT] – Hämtning och uppdatering av egna svar
    // ----------------------

    // GET: api/AssessmentItem/patient/user/{userId}
    // Returnerar en lista med bedömningar som tillhör en specifik patient
    [HttpGet("patient/user/{userId}")]
    public ActionResult<IEnumerable<object>> GetPatientAssessmentList(int userId)
    {
        var assessments = _context.Assessments
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.AssessmentID,
                a.CreatedAt,
                a.ScaleType,
                a.IsComplete
            })
            .ToList();

        return Ok(assessments);
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
                Flag = ai.Flag,
                SkippedByPatient = ai.SkippedByPatient
            })
            .ToList();

        return Ok(items);
    }

    // PUT: api/AssessmentItem/patient/{id}
    // Uppdaterar en patients eget svar
    [HttpPut("patient/{id}")]
    public IActionResult UpdatePatientAnswer(int id, [FromBody] PatientAnswerDto dto)
    {
        Console.WriteLine($"[INFO] PUT patient/{id} – inkommande answer: {dto.Answer}, kommentar: {dto.Comment}");

        var item = _context.AssessmentItems.Find(id);
        if (item == null)
        {
            Console.WriteLine($"[ERROR] Kunde inte hitta AssessmentItem med ID={id}");
            return NotFound();
        }

        // Spara patientens svar
        item.PatientAnswer = dto.Answer;
        item.PatientComment = dto.Comment;
        item.AnsweredAt = DateTime.UtcNow;
        item.SkippedByPatient = false;

        // Återställ eventuell SkippedByPatient-markering om den finns
        if (item.SkippedByPatient)
        {
            Console.WriteLine($"[DEBUG] Fråga var tidigare skippad – återställer SkippedByPatient till false.");
            item.SkippedByPatient = false;
        }

        try
        {
            _context.SaveChanges();
            Console.WriteLine($"[SUCCESS] Svar sparat för ItemID={id}: Answer={item.PatientAnswer}");
            return NoContent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Misslyckades med att spara svar för ItemID={id}: {ex.Message}");
            return StatusCode(500, "Kunde inte spara svaret.");
        }
    }


    // PUT: api/AssessmentItem/skip/{itemId}
    // Markerar en fråga som överhoppad av patienten
    [HttpPut("skip/{itemId}")]
    public IActionResult SkipQuestion(int itemId)
    {
        var item = _context.AssessmentItems.Find(itemId);
        if (item == null) return NotFound();

        item.SkippedByPatient = true;
        item.AnsweredAt = DateTime.UtcNow;

        _context.SaveChanges();
        return NoContent();
    }

    // GET: api/AssessmentItem/patient/assessment/{assessmentId}/overview
    // Returnerar översikt över en patients bedömning
    [HttpGet("patient/assessment/{assessmentId}/overview")]
    public async Task<ActionResult<AssessmentOverviewDto>> GetAssessmentOverview(int assessmentId)
    {
        var assessment = await _context.Assessments
            .Where(a => a.AssessmentID == assessmentId)
            .FirstOrDefaultAsync();

        if (assessment == null)
            return NotFound();

        var items = await _context.AssessmentItems
            .Where(ai => ai.AssessmentID == assessmentId)
            .Include(ai => ai.Question)
            .OrderBy(ai => ai.Order)
            .ToListAsync();

        var total = items.Count; // ✅ Lägg till detta

        var questions = items.Select(ai => new QuestionOverviewDto
        {
            ItemID = ai.ItemID, // ✅ För PUT
            QuestionId = ai.QuestionID,
            QuestionText = ai.Question?.QuestionText ?? "Frågetext saknas",
            PatientAnswer = ai.PatientAnswer is >= 0 and <= 4 ? ai.PatientAnswer : null,
            PatientComment = ai.PatientComment,
            Order = ai.Order,
            Total = total
        }).ToList();

        var overview = new AssessmentOverviewDto
        {
            AssessmentId = assessment.AssessmentID,
            ScaleType = assessment.ScaleType,
            IsComplete = assessment.IsComplete,
            CreatedAt = assessment.CreatedAt ?? DateTime.MinValue,
            Questions = questions
        };

        return Ok(overview);
    }




    // POST: api/AssessmentItem/assessment/{assessmentId}/complete
    // Markerar en hel bedömning som färdig
    [HttpPost("assessment/{assessmentId}/complete")]
    public IActionResult CompleteAssessment(int assessmentId)
    {
        var assessment = _context.Assessments.Find(assessmentId);
        if (assessment == null) return NotFound();

        assessment.IsComplete = true;
        _context.SaveChanges();

        return NoContent();
    }

    // ----------------------
    // [PERSONAL / ADMIN] – Full åtkomst till alla svar och åtgärder
    // ----------------------

    // GET: api/AssessmentItem
    // Returnerar alla bedömningsposter (inklusive personal- och patientsvar)
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
                Flag = ai.Flag,
                SkippedByPatient = ai.SkippedByPatient
            })
            .ToList();
    }

    // GET: api/AssessmentItem/staff/assessment/{assessmentId}/overview
    [HttpGet("staff/assessment/{assessmentId}/overview")]
    public async Task<ActionResult<StaffResultOverviewDto>> GetStaffAssessmentOverview(int assessmentId)
    {
        var assessment = await _context.Assessments
            .Include(a => a.User) // ✅ Inkludera användare för att få ut namnet
            .FirstOrDefaultAsync(a => a.AssessmentID == assessmentId);

        if (assessment == null)
            return NotFound();

        var items = await _context.AssessmentItems
            .Where(ai => ai.AssessmentID == assessmentId)
            .Include(ai => ai.Question)
            .OrderBy(ai => ai.Order)
            .ToListAsync();

        var dto = new StaffResultOverviewDto
        {
            AssessmentId = assessment.AssessmentID,
            UserId = assessment.UserId,
            Username = assessment.User?.Username ?? "Okänd", // ✅ Ny egenskap för att visa namn
            CreatedAt = assessment.CreatedAt ?? DateTime.MinValue,
            Questions = items.Select(ai => new StaffResultRowDto
            {
                ItemID = ai.ItemID,
                Order = ai.Order,
                QuestionText = ai.Question?.QuestionText ?? "Frågetext saknas",
                PatientAnswer = ai.PatientAnswer,
                StaffAnswer = ai.StaffAnswer,
                PatientComment = ai.PatientComment,
                StaffComment = ai.StaffComment,
                Flag = ai.Flag,
                SkippedByPatient = ai.SkippedByPatient
                // Difference beräknas automatiskt
            }).ToList()
        };

        return Ok(dto);
    }


    // GET: api/AssessmentItem/{id}
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
            Flag = item.Flag,
            SkippedByPatient = item.SkippedByPatient
        };
    }

    // GET: api/AssessmentItem/patient/assessment/{assessmentId}/question/{order}
    // Hämtar ett specifikt bedömningsitem
    [HttpGet("patient/assessment/{assessmentId}/question/{order}")]
    public async Task<ActionResult<QuestionDto>> GetQuestionByOrder(int assessmentId, int order)
    {
        var item = await _context.AssessmentItems
            .Include(i => i.Question)
            .FirstOrDefaultAsync(i => i.AssessmentID == assessmentId && i.Order == order);

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
            QuestionText = item.Question.QuestionText!,
            Category = item.Question.Category!,
            IsActive = item.Question.IsActive,
            Order = item.Order,
            Total = total,
            ScaleType = assessment?.ScaleType
        });
    }

    // POST: api/AssessmentItem
    // Skapar ett nytt bedömningsitem (t.ex. när formulär byggs manuellt)
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
            SkippedByPatient = false,
            AnsweredAt = DateTime.UtcNow
        };

        _context.AssessmentItems.Add(item);
        _context.SaveChanges();

        return CreatedAtAction(nameof(GetItem), new { id = item.ItemID }, dto);
    }

    // PUT: api/AssessmentItem/staff/{id}
    // Uppdaterar personals svar
    [HttpPut("staff/{id}")]
    public IActionResult UpdateStaffAnswer(int id, [FromBody] StaffAnswerDto dto)
    {
        var item = _context.AssessmentItems.Find(id);
        if (item == null) return NotFound();

        item.StaffAnswer = dto.Answer;
        item.StaffComment = dto.Comment;
        item.Flag = dto.Flag ?? false;
        item.AnsweredAt = DateTime.UtcNow;

        _context.SaveChanges();
        return NoContent();
    }

    // DELETE: api/AssessmentItem/{id}
    // Raderar ett bedömningsitem (om det t.ex. blivit fel)
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
