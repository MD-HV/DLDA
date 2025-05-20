using DLDA.GUI.DTOs.Assessment;
using DLDA.GUI.DTOs.Patient;
using System.Net.Http.Json;
using System.Text.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Serviceklass som hanterar hämtning av statistik för patienter.
    /// </summary>
    public class PatientStatisticsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PatientStatisticsService> _logger;

        public PatientStatisticsService(IHttpClientFactory factory, ILogger<PatientStatisticsService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Hämtar alla patientens svar för en bedömning.
        /// </summary>
        public async Task<List<PatientAnswerStatsDto>> GetAnswersForAssessmentAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"AssessmentItem/patient/assessment/{assessmentId}");
                if (!response.IsSuccessStatusCode) return new();

                return await response.Content.ReadFromJsonAsync<List<PatientAnswerStatsDto>>() ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetAnswersForAssessmentAsync({AssessmentId})", assessmentId);
                return new();
            }
        }

        /// <summary>
        /// Hämtar grundläggande metadata om en bedömning.
        /// </summary>
        public async Task<AssessmentDto?> GetAssessmentAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Assessment/{assessmentId}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<AssessmentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetAssessmentAsync({AssessmentId})", assessmentId);
                return null;
            }
        }

        /// <summary>
        /// Hämtar summerad statistik för en bedömning.
        /// </summary>
        public async Task<PatientSingleSummaryDto?> GetSummaryAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"statistics/summary/patient/{assessmentId}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<PatientSingleSummaryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetSummaryAsync({AssessmentId})", assessmentId);
                return null;
            }
        }

        /// <summary>
        /// Hämtar förbättringar över tid för en patient (minst 2 bedömningar krävs).
        /// </summary>
        public async Task<PatientChangeOverviewDto?> GetImprovementDataAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"statistics/patient-change-overview/{userId}");
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                if (json.Contains("inte tillräckligt")) return null;

                return JsonSerializer.Deserialize<PatientChangeOverviewDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetImprovementDataAsync({UserId})", userId);
                return null;
            }
        }
    }
}
