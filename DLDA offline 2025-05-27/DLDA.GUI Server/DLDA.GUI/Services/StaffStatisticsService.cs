using DLDA.GUI.DTOs.Assessment;
using DLDA.GUI.DTOs.Staff;
using System.Net.Http.Json;
using System.Text.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Service för att hantera statistik för personalvy.
    /// </summary>
    public class StaffStatisticsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StaffStatisticsService> _logger;

        public StaffStatisticsService(IHttpClientFactory factory, ILogger<StaffStatisticsService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Hämtar jämförelsedata mellan patient och personal samt bedömningsinfo.
        /// </summary>
        public async Task<(List<StaffStatistics>? Comparison, AssessmentDto? Assessment)> GetComparisonAsync(int assessmentId)
        {
            try
            {
                // Hämta jämförelsedata
                var comparisonResponse = await _httpClient.GetAsync($"statistics/comparison-table-staff/{assessmentId}");
                List<StaffStatistics>? comparison = null;
                if (comparisonResponse.IsSuccessStatusCode)
                {
                    var comparisonJson = await comparisonResponse.Content.ReadAsStringAsync();
                    comparison = JsonSerializer.Deserialize<List<StaffStatistics>>(comparisonJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                // Hämta bedömning
                var assessmentResponse = await _httpClient.GetAsync($"assessment/{assessmentId}");
                AssessmentDto? assessment = null;
                if (assessmentResponse.IsSuccessStatusCode)
                {
                    var assessmentJson = await assessmentResponse.Content.ReadAsStringAsync();
                    assessment = JsonSerializer.Deserialize<AssessmentDto>(assessmentJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                return (comparison, assessment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Undantag i GetComparisonAsync({AssessmentId})", assessmentId);
                return (null, null);
            }
        }

        /// <summary>
        /// Hämtar översiktsdata över tid för specifik patient.
        /// </summary>
        public async Task<StaffChangeOverviewDto?> GetChangeOverviewAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"statistics/staff-change-overview/{userId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Misslyckades att hämta översiktsdata: {StatusCode}", response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                if (json.Contains("inte tillräckligt")) return null;

                var overview = JsonSerializer.Deserialize<StaffChangeOverviewDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return overview;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Undantag i GetChangeOverviewAsync({UserId})", userId);
                return null;
            }
        }

        /// <summary>
        /// Hämtar patientens egna svar för en bedömning (används för sammanställning).
        /// </summary>
        public async Task<List<StaffStatistics>> GetPatientAnswerSummaryAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"statistics/patient-answer-summary/{assessmentId}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Misslyckades att hämta patientens svarssammanställning: {StatusCode}", response.StatusCode);
                    return new();
                }

                var json = await response.Content.ReadAsStringAsync();
                var stats = JsonSerializer.Deserialize<List<StaffStatistics>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return stats ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Undantag i GetPatientAnswerSummaryAsync({AssessmentId})", assessmentId);
                return new();
            }
        }

        /// <summary>
        /// Jämför två valda avslutade personalbedömningar för en patient.
        /// </summary>
        public async Task<StaffChangeOverviewDto?> CompareAssessmentsAsync(int firstId, int secondId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"statistics/staff-compare-assessments/{firstId}/{secondId}");
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                if (json.Contains("inte tillräckligt")) return null;

                return JsonSerializer.Deserialize<StaffChangeOverviewDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid CompareAssessmentsAsync({FirstId}, {SecondId})", firstId, secondId);
                return null;
            }
        }
    }
}
