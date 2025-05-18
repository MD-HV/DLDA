using DLDA.GUI.DTOs.Staff;
using System.Net.Http.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Service för att hantera personalsammanställning av en bedömning.
    /// </summary>
    public class StaffResultService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StaffResultService> _logger;

        public StaffResultService(IHttpClientFactory factory, ILogger<StaffResultService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Hämtar översikten av en personals bedömning.
        /// </summary>
        public async Task<StaffResultOverviewDto?> GetOverviewAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"AssessmentItem/staff/assessment/{assessmentId}/overview");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<StaffResultOverviewDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetOverviewAsync({AssessmentId})", assessmentId);
                return null;
            }
        }

        /// <summary>
        /// Uppdaterar ett personal-svar för en fråga.
        /// </summary>
        public async Task<bool> UpdateStaffAnswerAsync(SubmitStaffAnswerDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("Question/quiz/staff/submit", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid UpdateStaffAnswerAsync för ItemID={ItemID}", dto.ItemID);
                return false;
            }
        }

        /// <summary>
        /// Markerar en bedömning som klar från personalens sida.
        /// </summary>
        public async Task<bool> CompleteStaffAssessmentAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"AssessmentItem/assessment/{assessmentId}/staff-complete", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid CompleteStaffAssessmentAsync({AssessmentId})", assessmentId);
                return false;
            }
        }

        /// <summary>
        /// Låser upp en tidigare markerad bedömning.
        /// </summary>
        public async Task<bool> UnlockAssessmentAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"assessment/unlock/{assessmentId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid UnlockAssessmentAsync({AssessmentId})", assessmentId);
                return false;
            }
        }
    }
}
