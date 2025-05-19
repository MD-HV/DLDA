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
                var comparisonResponse = await _httpClient.GetAsync($"statistics/comparison-table-staff/{assessmentId}");
                if (!comparisonResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Misslyckades att hämta jämförelsedata: {StatusCode}", comparisonResponse.StatusCode);
                    return (null, null);
                }

                var comparisonJson = await comparisonResponse.Content.ReadAsStringAsync();
                var comparison = JsonSerializer.Deserialize<List<StaffStatistics>>(comparisonJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var assessmentResponse = await _httpClient.GetAsync($"assessment/{assessmentId}");
                if (!assessmentResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Misslyckades att hämta bedömningsinfo: {StatusCode}", assessmentResponse.StatusCode);
                    return (comparison, null); // comparison kan vara null eller ej
                }

                var assessmentJson = await assessmentResponse.Content.ReadAsStringAsync();
                var assessment = JsonSerializer.Deserialize<AssessmentDto>(assessmentJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
    }
}
