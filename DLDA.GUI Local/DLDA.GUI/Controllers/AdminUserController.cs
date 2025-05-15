using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace DLDA.GUI.Controllers
{
    [RoleAuthorize("admin")]
    public class AdminUserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdminUserController> _logger;

        public AdminUserController(IHttpClientFactory httpClientFactory, ILogger<AdminUserController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("DLDA");
            _logger = logger;
        }

        // GET: /AdminUser
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("User");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Kunde inte hämta användare. Statuskod: {StatusCode}", response.StatusCode);
                    return View("Error");
                }

                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<UserDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av användarlista.");
                return View("Error");
            }
        }

        // GET: /AdminUser/Create
        public IActionResult Create()
        {
            return View(new UserDto());
        }

        // POST: /AdminUser/Create
        [HttpPost]
        public async Task<IActionResult> Create(UserDto user)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Ogiltig modell för skapande: {@User}", user);
                return View(user);
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("User", user);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Användaren skapades.";
                    return RedirectToAction("Index");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Misslyckades skapa användare. Status: {StatusCode}, Svar: {Response}", response.StatusCode, errorContent);
                TempData["Error"] = "Det gick inte att skapa användaren.";
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Undantag vid skapande av användare: {@User}", user);
                TempData["Error"] = "Ett oväntat fel uppstod.";
                return View(user);
            }
        }

        // GET: /AdminUser/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"User/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Kunde inte hämta användare för redigering. ID: {Id}, Status: {StatusCode}", id, response.StatusCode);
                    return RedirectToAction("Index");
                }

                var json = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av användare för redigering. ID: {Id}", id);
                return RedirectToAction("Index");
            }
        }

        // POST: /AdminUser/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, UserDto user)
        {
            if (id != user.UserID)
            {
                _logger.LogWarning("Felaktigt ID vid redigering. URL-ID: {Id}, Model-ID: {UserId}", id, user.UserID);
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Ogiltig modell vid redigering: {@User}", user);
                return View(user);
            }

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"User/{id}", user);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Användaren uppdaterades.";
                    return RedirectToAction("Index");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Misslyckades uppdatera användare ID {Id}. Status: {StatusCode}, Svar: {Response}", id, response.StatusCode, errorContent);
                TempData["Error"] = "Kunde inte uppdatera användaren.";
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Undantag vid uppdatering av användare ID: {Id}", id);
                TempData["Error"] = "Ett oväntat fel uppstod.";
                return View(user);
            }
        }

        // POST: /AdminUser/DeleteUserConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int userID)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"User/{userID}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Användaren togs bort.";
                    return RedirectToAction("Index");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Misslyckades ta bort användare ID {Id}. Status: {StatusCode}, Svar: {Response}", userID, response.StatusCode, errorContent);
                TempData["Error"] = "Kunde inte ta bort användaren.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid borttagning av användare ID: {Id}", userID);
                TempData["Error"] = "Ett oväntat fel uppstod.";
                return RedirectToAction("Index");
            }
        }

        // GET: /AdminUser/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"User/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Kunde inte hämta användardata för borttagning.";
                    return RedirectToAction("Index");
                }

                var json = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View(user); // 👈 detta kräver att du har en Delete.cshtml-vy!
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid laddning av användare för borttagning. ID: {Id}", id);
                TempData["Error"] = "Ett oväntat fel uppstod.";
                return RedirectToAction("Index");
            }
        }
    }
}
