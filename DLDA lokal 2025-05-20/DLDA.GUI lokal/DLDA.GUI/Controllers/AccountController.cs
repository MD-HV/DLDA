using Microsoft.AspNetCore.Mvc;
using DLDA.GUI.DTOs.Authentication;

/// <summary>
/// Hanterar inloggning och utloggning av användare.
/// </summary>
public class AccountController : Controller
{
    private readonly AccountService _accountService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(AccountService accountService, ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    /// <summary>
    /// Visar inloggningssidan.
    /// </summary>
    [HttpGet]
    public IActionResult Login() => View();

    /// <summary>
    /// Bearbetar inloggningsuppgifter och skapar session.
    /// </summary>
    /// <param name="login">Inloggningsdata (användarnamn och lösenord).</param>
    /// <returns>Redirect till relevant startsida baserat på användarroll, eller tillbaks till login vid fel.</returns>
    [HttpPost]
    public async Task<IActionResult> Login(LoginDto login)
    {
        var user = await _accountService.LoginAsync(login);
        if (user == null)
        {
            ViewBag.Error = "Felaktigt användarnamn, lösenord eller serverfel.";
            return View();
        }

        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("Role", user.Role);

        return user.Role.ToLower() switch
        {
            "admin" => RedirectToAction("Index", "Admin"),
            "staff" => RedirectToAction("Index", "StaffAssessment"),
            "patient" => RedirectToAction("Index", "PatientAssessment"),
            _ => RedirectToAction("Login")
        };
    }

    /// <summary>
    /// Loggar ut användaren genom att rensa sessionen.
    /// </summary>
    /// <returns>Redirect till inloggningssidan.</returns>
    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
