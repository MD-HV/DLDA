using DLDA.GUI.DTOs.Assessment;
using System.Net.Http.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Serviceklass för att hämta patientens bedömningar via API.
    /// </summary>
    public class PatientAssessmentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PatientAssessmentService> _logger;

        public PatientAssessmentService(IHttpClientFactory factory, ILogger<PatientAssessmentService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Hämtar alla bedömningar för en given användare.
        /// </summary>
        public async Task<List<AssessmentDto>> GetAssessmentsForUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Assessment/user/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("API-svar misslyckades vid hämtning av bedömningar för patient {UserId}. Status: {StatusCode}",
                        userId, response.StatusCode);
                    return new List<AssessmentDto>();
                }

                var result = await response.Content.ReadFromJsonAsync<List<AssessmentDto>>();
                return result ?? new List<AssessmentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid API-anrop: GetAssessmentsForUserAsync({UserId})", userId);
                return new List<AssessmentDto>();
            }
        }
    }
}
