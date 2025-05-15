using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using DLDA.GUI.DTOs;

public class AccountController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IHttpClientFactory factory, ILogger<AccountController> logger)
    {
        _httpClient = factory.CreateClient("DLDA");
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto login)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/login", login);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Inloggning misslyckades. Status: {Status}", response.StatusCode);
                ViewBag.Error = "Felaktigt användarnamn eller lösenord.";
                return View();
            }

            var user = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (user == null)
            {
                _logger.LogError("Inloggningssvaret kunde inte deserialiseras.");
                ViewBag.Error = "Felaktigt svar från servern.";
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ett fel uppstod vid inloggning.");
            ViewBag.Error = "Det gick inte att kontakta servern. Försök igen senare.";
            return View();
        }
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
