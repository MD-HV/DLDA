using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace DLDA.GUI.Controllers
{
    [RoleAuthorize("patient")]
    public class PatientQuizController : Controller
    {
        private readonly HttpClient _httpClient;

        public PatientQuizController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DLDA");
        }

        [HttpGet]
        public async Task<IActionResult> Info(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Assessment/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Kunde inte hämta information om bedömningen.";
                    return View("Error");
                }

                var assessment = await response.Content.ReadFromJsonAsync<AssessmentDto>();
                ViewBag.AssessmentId = id;
                ViewBag.HasStarted = assessment?.HasStarted ?? false;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] Info: {ex.Message}");
                TempData["Error"] = "Ett tekniskt fel uppstod.";
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ScaleSelect(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Assessment/{id}");
                if (!response.IsSuccessStatusCode)
                    return View("Error");

                var assessment = await response.Content.ReadFromJsonAsync<AssessmentDto>();
                return View(assessment);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] ScaleSelect: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetScale(int id, string selectedScale)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Assessment/{id}");
                if (!response.IsSuccessStatusCode)
                    return View("Error");

                var dto = await response.Content.ReadFromJsonAsync<AssessmentDto>();
                dto!.ScaleType = selectedScale;

                var update = await _httpClient.PutAsJsonAsync($"Assessment/{id}", dto);
                if (!update.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Kunde inte spara vald skala.";
                    return RedirectToAction("ScaleSelect", new { id });
                }

                return RedirectToAction("Resume", new { id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] SetScale: {ex.Message}");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Resume(int id)
        {
            try
            {
                var assessmentResponse = await _httpClient.GetAsync($"Assessment/{id}");
                if (!assessmentResponse.IsSuccessStatusCode)
                    return View("Error");

                var assessment = await assessmentResponse.Content.ReadFromJsonAsync<AssessmentDto>();
                if (assessment?.IsComplete == true)
                    return RedirectToAction("Index", "PatientResult", new { assessmentId = id });

                var response = await _httpClient.GetAsync($"Question/quiz/patient/next/{id}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "Du har svarat på alla frågor.";
                    return RedirectToAction("Index", "PatientResult", new { assessmentId = id });
                }

                if (!response.IsSuccessStatusCode)
                    return View("Error");

                var question = await response.Content.ReadFromJsonAsync<QuestionDto>();
                ViewBag.AssessmentId = id;
                return View("Question", question);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] Resume: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(int itemId, int assessmentId, int answer, string? comment)
        {
            try
            {
                var dto = new PatientAnswerDto
                {
                    Answer = answer,
                    Comment = string.IsNullOrWhiteSpace(comment) ? null : comment
                };

                var response = await _httpClient.PutAsJsonAsync($"AssessmentItem/patient/{itemId}", dto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERROR] SubmitAnswer: {response.StatusCode} - {errorContent}");
                    TempData["Error"] = "Kunde inte spara svaret.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] SubmitAnswer: {ex.Message}");
                TempData["Error"] = "Ett tekniskt fel uppstod vid svar.";
            }

            return RedirectToAction("Resume", new { id = assessmentId });
        }

        [HttpPost]
        public async Task<IActionResult> SkipQuestion(int itemId, int assessmentId)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"AssessmentItem/skip/{itemId}", new { });
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ERROR] SkipQuestion: {response.StatusCode} - {content}");
                    TempData["Error"] = "Kunde inte hoppa över frågan.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] SkipQuestion: {ex.Message}");
                TempData["Error"] = "Ett tekniskt fel uppstod vid överhopp.";
            }

            return RedirectToAction("Resume", new { id = assessmentId });
        }

        [HttpPost]
        public async Task<IActionResult> Previous(int assessmentId, int currentOrder)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Question/quiz/patient/previous/{assessmentId}/{currentOrder}");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Kunde inte hämta föregående fråga.";
                    return RedirectToAction("Resume", new { id = assessmentId });
                }

                var question = await response.Content.ReadFromJsonAsync<QuestionDto>();
                ViewBag.AssessmentId = assessmentId;
                return View("Question", question);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] Previous: {ex.Message}");
                TempData["Error"] = "Ett tekniskt fel uppstod vid hämtning av föregående fråga.";
                return RedirectToAction("Resume", new { id = assessmentId });
            }
        }

        [HttpPost]
        public IActionResult Pause(int assessmentId)
        {
            TempData["Success"] = "Du kan återuppta din bedömning senare.";
            return RedirectToAction("Index", "PatientAssessment");
        }
    }
}
