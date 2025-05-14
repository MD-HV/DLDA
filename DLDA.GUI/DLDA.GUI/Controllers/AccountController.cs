using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using DLDA.GUI.DTOs;

public class AccountController : Controller
{
    private readonly HttpClient _httpClient;

    public AccountController(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("DLDA");
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto login)
    {
        var response = await _httpClient.PostAsJsonAsync("Auth/login", login);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Felaktigt användarnamn eller lösenord.";
            return View();
        }

        var user = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        HttpContext.Session.SetInt32("UserID", user!.UserID);
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

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
