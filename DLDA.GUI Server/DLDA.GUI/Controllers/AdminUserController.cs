using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs.User;
using DLDA.GUI.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller för att administrera användare (CRUD).
/// </summary>
[RoleAuthorize("admin")]
public class AdminUserController : Controller
{
    private readonly UserAdminService _service;

    /// <summary>
    /// Skapar en instans av AdminUserController.
    /// </summary>
    public AdminUserController(UserAdminService service)
    {
        _service = service;
    }

    /// <summary>
    /// Visar en lista med alla användare.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var users = await _service.GetAllAsync();
        return View(users);
    }

    /// <summary>
    /// Visar formulär för att skapa ny användare.
    /// </summary>
    public IActionResult Create() => View(new UserDto());

    /// <summary>
    /// Skapar en ny användare.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(UserDto user)
    {
        if (!ModelState.IsValid) return View(user);

        var success = await _service.CreateAsync(user);
        TempData[success ? "Success" : "Error"] = success
            ? "Användaren skapades."
            : "Det gick inte att skapa användaren.";
        return success ? RedirectToAction("Index") : View(user);
    }

    /// <summary>
    /// Visar redigeringsformulär för angiven användare.
    /// </summary>
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _service.GetByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "Kunde inte hämta användaren.";
            return RedirectToAction("Index");
        }

        return View(user);
    }

    /// <summary>
    /// Uppdaterar en användare.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Edit(int id, UserDto user)
    {
        if (id != user.UserID) return BadRequest();
        if (!ModelState.IsValid) return View(user);

        var success = await _service.UpdateAsync(id, user);
        TempData[success ? "Success" : "Error"] = success
            ? "Användaren uppdaterades."
            : "Kunde inte uppdatera användaren.";
        return success ? RedirectToAction("Index") : View(user);
    }

    /// <summary>
    /// Visar bekräftelse för att ta bort en användare.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _service.GetByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "Kunde inte hämta användaren.";
            return RedirectToAction("Index");
        }

        return View(user);
    }

    /// <summary>
    /// Tar bort användaren permanent.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(int userID)
    {
        var success = await _service.DeleteAsync(userID);
        TempData[success ? "Success" : "Error"] = success
            ? "Användaren togs bort."
            : "Kunde inte ta bort användaren.";
        return RedirectToAction("Index");
    }
}
