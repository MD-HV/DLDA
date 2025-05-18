using DLDA.GUI.DTOs.Assessment;
using DLDA.GUI.DTOs.Patient;
using DLDA.GUI.DTOs.Question;
using System.Net.Http.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Serviceklass som hanterar quizflödet för patienter (frågor, svar, hopp, skala).
    /// </summary>
    public class PatientQuizService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PatientQuizService> _logger;

        public PatientQuizService(IHttpClientFactory factory, ILogger<PatientQuizService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Hämtar en bedömning baserat på ID.
        /// </summary>
        public async Task<AssessmentDto?> GetAssessmentAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Assessment/{id}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<AssessmentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetAssessmentAsync({Id})", id);
                return null;
            }
        }

        /// <summary>
        /// Uppdaterar skaltyp för en specifik bedömning.
        /// </summary>
        public async Task<bool> UpdateScaleAsync(int id, string scale)
        {
            var dto = await GetAssessmentAsync(id);
            if (dto == null) return false;

            dto.ScaleType = scale;

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"Assessment/{id}", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid UpdateScaleAsync({Id})", id);
                return false;
            }
        }

        /// <summary>
        /// Hämtar nästa obesvarade fråga i quizflödet för en given bedömning.
        /// </summary>
        public async Task<QuestionDto?> GetNextQuestionAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Question/quiz/patient/next/{assessmentId}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<QuestionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetNextQuestionAsync({AssessmentId})", assessmentId);
                return null;
            }
        }

        /// <summary>
        /// Hämtar föregående fråga utifrån aktuell position i bedömningen.
        /// </summary>
        public async Task<QuestionDto?> GetPreviousQuestionAsync(int assessmentId, int order)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Question/quiz/patient/previous/{assessmentId}/{order}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<QuestionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetPreviousQuestionAsync({AssessmentId}, {Order})", assessmentId, order);
                return null;
            }
        }

        /// <summary>
        /// Skickar in patientens svar och eventuell kommentar för en fråga.
        /// </summary>
        public async Task<bool> SubmitAnswerAsync(int itemId, PatientAnswerDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"AssessmentItem/patient/{itemId}", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid SubmitAnswerAsync({ItemId})", itemId);
                return false;
            }
        }

        /// <summary>
        /// Markerar en fråga som överhoppad.
        /// </summary>
        public async Task<bool> SkipQuestionAsync(int itemId)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"AssessmentItem/skip/{itemId}", new { });
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid SkipQuestionAsync({ItemId})", itemId);
                return false;
            }
        }
    }
}
