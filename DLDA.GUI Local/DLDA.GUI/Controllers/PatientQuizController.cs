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

        // GET: /PatientQuiz/Info/{id}
        // Visar informationssidan för quizstart eller fortsättning
        [HttpGet]
        public async Task<IActionResult> Info(int id)
        {
            var response = await _httpClient.GetAsync($"Assessment/{id}");
            if (!response.IsSuccessStatusCode)
                return View("Error");

            var assessment = await response.Content.ReadFromJsonAsync<AssessmentDto>();
            ViewBag.AssessmentId = id;
            ViewBag.HasStarted = assessment?.HasStarted ?? false;

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
            // 1. Hämta bedömningen
            var assessmentResponse = await _httpClient.GetAsync($"Assessment/{id}");
            if (!assessmentResponse.IsSuccessStatusCode)
                return View("Error");

            var assessment = await assessmentResponse.Content.ReadFromJsonAsync<AssessmentDto>();

            // ✅ Om bedömningen är klar, visa resultat
            if (assessment?.IsComplete == true)
            {
                return RedirectToAction("Index", "PatientResult", new { assessmentId = id });
            }

            // 2. Hämta nästa obesvarade fråga
            var response = await _httpClient.GetAsync($"Question/quiz/patient/next/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["Error"] = "Du har svarat på alla frågor. Klicka på 'Markera som klar' på översiktssidan.";
                return RedirectToAction("Index", "PatientResult", new { assessmentId = id });
            }

            if (!response.IsSuccessStatusCode)
                return View("Error");

            var question = await response.Content.ReadFromJsonAsync<QuestionDto>();
            if (question == null)
                return RedirectToAction("Index", "PatientResult", new { assessmentId = id });

            ViewBag.AssessmentId = id;
            return View("Question", question);
        }

        // POST: /PatientQuiz/SubmitAnswer
        // Sparar svaret på en fråga
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(int itemId, int assessmentId, int answer, string? comment)
        {
            Console.WriteLine($"[DEBUG] SubmitAnswer: itemId={itemId}, assessmentId={assessmentId}, answer={answer}, comment={comment}");

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
                    Console.WriteLine($"[ERROR] Misslyckades att spara svar: {response.StatusCode} - {errorContent}");
                    TempData["Error"] = "Kunde inte spara svaret.";
                }
                else
                {
                    Console.WriteLine($"[SUCCESS] Svar sparades för itemId={itemId}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] SubmitAnswer: {ex.Message}");
                TempData["Error"] = "Ett tekniskt fel uppstod när svaret skulle sparas.";
            }

            return RedirectToAction("Resume", new { id = assessmentId });
        }


        // POST: /PatientQuiz/Skip
        // Markerar en fråga som överhoppad
        [HttpPost]
        public async Task<IActionResult> SkipQuestion(int itemId, int assessmentId)
        {
            Console.WriteLine($"[DEBUG] SkipQuestion: itemId={itemId}, assessmentId={assessmentId}");

            var response = await _httpClient.PutAsJsonAsync($"AssessmentItem/skip/{itemId}", new { });

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR] Misslyckades att hoppa över fråga: {response.StatusCode} - {content}");
                TempData["Error"] = "Kunde inte hoppa över frågan.";
            }
            else
            {
                Console.WriteLine($"[SUCCESS] Fråga markerad som skippad: itemId={itemId}");
            }

            return RedirectToAction("Resume", new { id = assessmentId });
        }

        // POST: /PatientQuiz/Previous
        // Hämtar och visar föregående fråga i bedömningen baserat på frågans ordning (Order)
        [HttpPost]
        public async Task<IActionResult> Previous(int assessmentId, int currentOrder)
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

        // POST: /PatientQuiz/Pause
        // Pausar quiz och återgår till översikt
        [HttpPost]
        public IActionResult Pause(int assessmentId)
        {
            TempData["Success"] = "Du kan återuppta din bedömning senare.";
            return RedirectToAction("Index", "PatientAssessment");
        }
    }
}
