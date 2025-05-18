using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs.Question;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Admincontroller för att hantera skapande, redigering och borttagning av frågor.
/// </summary>
[RoleAuthorize("admin")]
public class AdminQuestionController : Controller
{
    private readonly QuestionAdminService _service;

    /// <summary>
    /// Konstruktor som injicerar QuestionAdminService.
    /// </summary>
    public AdminQuestionController(QuestionAdminService service)
    {
        _service = service;
    }

    /// <summary>
    /// Visar en lista av alla frågor.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var questions = await _service.GetAllQuestionsAsync();
        return View("Index", questions);
    }

    /// <summary>
    /// Visar vyn för att skapa en ny fråga.
    /// </summary>
    public IActionResult Create() => View("Create", new QuestionDto());

    /// <summary>
    /// Skapar en ny fråga.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(QuestionDto dto)
    {
        if (!ModelState.IsValid) return View("Create", dto);

        var success = await _service.CreateQuestionAsync(dto);
        TempData[success ? "Success" : "Error"] = success
            ? "Frågan skapades."
            : "Kunde inte skapa frågan.";
        return success ? RedirectToAction("Index") : View("Create", dto);
    }

    /// <summary>
    /// Visar redigeringsvyn för en viss fråga.
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        var question = await _service.GetQuestionByIdAsync(id);
        if (question == null)
        {
            TempData["Error"] = "Kunde inte hitta frågan.";
            return RedirectToAction("Index");
        }

        return View("Edit", question);
    }

    /// <summary>
    /// Uppdaterar en fråga.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Edit(int id, QuestionDto dto)
    {
        if (id != dto.QuestionID) return BadRequest();
        if (!ModelState.IsValid) return View("Edit", dto);

        var success = await _service.UpdateQuestionAsync(id, dto);
        TempData[success ? "Success" : "Error"] = success
            ? "Frågan uppdaterades."
            : "Kunde inte uppdatera frågan.";
        return success ? RedirectToAction("Index") : View("Edit", dto);
    }

    /// <summary>
    /// Visar bekräftelsesidan för borttagning av en fråga.
    /// </summary>
    public async Task<IActionResult> Delete(int id)
    {
        var question = await _service.GetQuestionByIdAsync(id);
        if (question == null)
        {
            TempData["Error"] = "Kunde inte hitta frågan.";
            return RedirectToAction("Index");
        }

        return View("Delete", question);
    }

    /// <summary>
    /// Bekräftar och tar bort frågan.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var success = await _service.DeleteQuestionAsync(id);
        TempData[success ? "Success" : "Error"] = success
            ? "Frågan togs bort."
            : "Kunde inte ta bort frågan.";
        return RedirectToAction("Index");
    }
}
