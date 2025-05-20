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

        public async Task<bool> SkipQuestionAsync(int itemId, string? comment, bool flag)
        {
            var dto = new SubmitStaffAnswerDto
            {
                ItemID = itemId,
                Answer = null, 
                Comment = comment,
                Flag = flag
            };

            var response = await _httpClient.PostAsJsonAsync("question/quiz/staff/submit", dto);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Hämtar totalt antal frågor i ett assessment för personalen (baserat på Order).
        /// </summary>
        public async Task<int?> GetTotalQuestionCountForStaffAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"assessment/{assessmentId}/question-count");

                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                return int.TryParse(content, out var count) ? count : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetTotalQuestionCountForStaffAsync för assessment {AssessmentId}", assessmentId);
                return null;
            }
        }


    }
}
