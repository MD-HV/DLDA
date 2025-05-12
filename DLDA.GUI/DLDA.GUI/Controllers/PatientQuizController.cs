using DLDA.GUI.Authorization;
using DLDA.GUI.DTOs;
using Microsoft.AspNetCore.Mvc;
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

        // GET: /PatientQuiz/Info
        // Visar informationssidan före quizstart
        public IActionResult Info()
        {
            return View(); // Views/PatientQuiz/Info.cshtml
        }

        // GET: /PatientQuiz/ScaleSelect/{id}
        // Visar valet av skattningsskala för en ny bedömning
        [HttpGet]
        public async Task<IActionResult> ScaleSelect(int id)
        {
            var response = await _httpClient.GetAsync($"Assessment/{id}");
            if (!response.IsSuccessStatusCode)
                return View("Error");
            Console.WriteLine($"[DEBUG] ScaleSelect ID: {id}");

            var assessment = await response.Content.ReadFromJsonAsync<AssessmentDto>();
            return View(assessment); // skickar modellen till Views/PatientQuiz/ScaleSelect.cshtml
        }

        // POST: /PatientQuiz/SetScale
        // Sparar vald skattningsskala och går vidare till quiz
        [HttpPost]
        public async Task<IActionResult> SetScale(int id, string selectedScale)
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

            return RedirectToAction("Resume", new { id }); // 👉 går till första obesvarade fråga
        }

        // GET: /PatientQuiz/Resume/{id}
        // Hämtar nästa obesvarade fråga
        [HttpGet]
        public async Task<IActionResult> Resume(int id)
        {
            var response = await _httpClient.GetAsync($"Question/quiz/patient/next/{id}");
            if (!response.IsSuccessStatusCode)
                return View("Error");

            var question = await response.Content.ReadFromJsonAsync<QuestionDto>();
            if (question == null)
                return RedirectToAction("Index", "PatientAssessment"); // ✔️ Alla frågor klara
            Console.WriteLine($"[DEBUG] Question Order={question?.Order}, Total={question?.Total}");

            ViewBag.AssessmentId = id;
            return View("Question", question);
        }

        // POST: /PatientQuiz/SubmitAnswer
        // Sparar svaret på en fråga
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(int itemId, int assessmentId, int answer, string? comment)
        {
            var response = await _httpClient.PutAsJsonAsync($"AssessmentItem/patient/{itemId}", new
            {
                answer,
                comment
            });

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Kunde inte spara svaret.";
            }

            return RedirectToAction("Resume", new { id = assessmentId });
        }

        // POST: /PatientQuiz/Skip
        // Markerar en fråga som överhoppad
        [HttpPost]
        public async Task<IActionResult> SkipQuestion(int itemId, int assessmentId)
        {
            var response = await _httpClient.PutAsJsonAsync($"AssessmentItem/skip/{itemId}", new { });

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Kunde inte hoppa över frågan.";
            }

            return RedirectToAction("Resume", new { id = assessmentId });
        }

        // POST: /PatientQuiz/Previous
        // Hämtar och visar föregående fråga i bedömningen baserat på frågans ordning (Order).
        [HttpPost]
        public async Task<IActionResult> Previous(int assessmentId, int currentOrder)
        {
            var response = await _httpClient.GetAsync($"Question/quiz/patient/previous/{assessmentId}/{currentOrder}");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Resume", new { id = assessmentId });

            var question = await response.Content.ReadFromJsonAsync<QuestionDto>();
            ViewBag.AssessmentId = assessmentId;
            return View("Question", question);
        }


        [HttpPost]
        public IActionResult Pause(int assessmentId)
        {
            // Till exempel: gå till startsida eller översikt
            TempData["Success"] = "Du kan återuppta din bedömning senare.";
            return RedirectToAction("Index", "PatientAssessment");
        }
    }
}
