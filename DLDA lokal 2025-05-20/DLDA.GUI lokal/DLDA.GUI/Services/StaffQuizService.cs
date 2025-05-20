using DLDA.GUI.DTOs.Staff;
using System.Net.Http.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Serviceklass för att hantera personalens frågeflöde (quiz) i en bedömning.
    /// </summary>
    public class StaffQuizService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StaffQuizService> _logger;

        public StaffQuizService(IHttpClientFactory factory, ILogger<StaffQuizService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Hämtar nästa fråga för personalen i en bedömning.
        /// </summary>
        public async Task<StaffQuestionDto?> GetNextQuestionAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Question/quiz/staff/next/{assessmentId}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<StaffQuestionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetNextQuestionAsync({AssessmentId})", assessmentId);
                return null;
            }
        }

        /// <summary>
        /// Hämtar föregående fråga baserat på ordning.
        /// </summary>
        public async Task<StaffQuestionDto?> GetPreviousQuestionAsync(int assessmentId, int order)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Question/quiz/staff/previous/{assessmentId}/{order}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<StaffQuestionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetPreviousQuestionAsync({AssessmentId}, {Order})", assessmentId, order);
                return null;
            }
        }

        /// <summary>
        /// Skickar in ett svar från personalen.
        /// </summary>
        public async Task<bool> SubmitAnswerAsync(SubmitStaffAnswerDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("Question/quiz/staff/submit", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid SubmitAnswerAsync för ItemID={ItemId}", dto.ItemID);
                return false;
            }
        }
    }
}
