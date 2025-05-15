using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace DLDA.GUI.Controllers
{
    [RoleAuthorize("admin")]
    public class AdminQuestionController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdminQuestionController> _logger;

        public AdminQuestionController(IHttpClientFactory httpClientFactory, ILogger<AdminQuestionController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("DLDA");
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("Question");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API-svaret misslyckades med status: {Status}", response.StatusCode);
                    TempData["Error"] = "Kunde inte hämta frågorna från API:t.";
                    return View("Index", new List<QuestionDto>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var questions = JsonSerializer.Deserialize<List<QuestionDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View("Index", questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid anrop till API:t.");
                TempData["Error"] = "Ett fel uppstod vid kontakt med API:t.";
                return View("Index", new List<QuestionDto>());
            }
        }

        public IActionResult Create()
        {
            return View("Create", new QuestionDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(QuestionDto dto)
        {
            if (!ModelState.IsValid) return View("Create", dto);

            try
            {
                var response = await _httpClient.PostAsJsonAsync("Question", dto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Frågan skapades.";
                    return RedirectToAction("Index");
                }

                TempData["Error"] = "Kunde inte skapa frågan.";
                return View("Create", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid försök att skapa fråga.");
                TempData["Error"] = "API-fel vid skapande.";
                return View("Create", dto);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Question/{id}");
                if (!response.IsSuccessStatusCode) return RedirectToAction("Index");

                var json = await response.Content.ReadAsStringAsync();
                var question = JsonSerializer.Deserialize<QuestionDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View("Edit", question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kunde inte ladda fråga ID {Id}.", id);
                TempData["Error"] = "API-fel vid laddning av fråga.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, QuestionDto dto)
        {
            if (id != dto.QuestionID) return BadRequest();
            if (!ModelState.IsValid) return View("Edit", dto);

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"Question/{id}", dto);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Frågan uppdaterades.";
                    return RedirectToAction("Index");
                }

                TempData["Error"] = "Kunde inte uppdatera frågan.";
                return View("Edit", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API-fel vid uppdatering av fråga ID {Id}.", id);
                TempData["Error"] = "Ett fel uppstod vid kontakt med API:t.";
                return View("Edit", dto);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Question/{id}");
                if (!response.IsSuccessStatusCode) return RedirectToAction("Index");

                var json = await response.Content.ReadAsStringAsync();
                var question = JsonSerializer.Deserialize<QuestionDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return View("Delete", question);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API-fel vid laddning av fråga för borttagning ID {Id}.", id);
                TempData["Error"] = "Ett fel uppstod vid hämtning av fråga.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Question/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Frågan togs bort.";
                    return RedirectToAction("Index");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Misslyckades ta bort fråga ID {Id}. Status: {StatusCode}, Svar: {Response}", id, response.StatusCode, errorContent);
                TempData["Error"] = "Kunde inte ta bort frågan.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API-fel vid borttagning av fråga ID {Id}.", id);
                TempData["Error"] = "Ett fel uppstod vid borttagning.";
                return RedirectToAction("Index");
            }
        }
    }
}
