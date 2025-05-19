using DLDA.GUI.DTOs.Assessment;
using DLDA.GUI.DTOs.Patient;
using System.Net.Http.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Serviceklass som hanterar resultatöversikt och slutförande av patientens bedömning.
    /// </summary>
    public class PatientResultService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PatientResultService> _logger;

        public PatientResultService(IHttpClientFactory factory, ILogger<PatientResultService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Hämtar metadata om en bedömning.
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
        /// Hämtar övergripande svar och kommentarer för en bedömning.
        /// </summary>
        public async Task<AssessmentOverviewDto?> GetOverviewAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"AssessmentItem/patient/assessment/{assessmentId}/overview");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<AssessmentOverviewDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid GetOverviewAsync({AssessmentId})", assessmentId);
                return null;
            }
        }

        /// <summary>
        /// Markerar en bedömning som slutförd.
        /// </summary>
        public async Task<bool> CompleteAssessmentAsync(int assessmentId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"AssessmentItem/assessment/{assessmentId}/complete", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid CompleteAssessmentAsync({AssessmentId})", assessmentId);
                return false;
            }
        }

        /// <summary>
        /// Uppdaterar ett tidigare inskickat svar för en bedömningsfråga.
        /// </summary>
        public async Task<bool> UpdateAnswerAsync(int itemId, PatientAnswerDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"AssessmentItem/patient/{itemId}", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid UpdateAnswerAsync({ItemId})", itemId);
                return false;
            }
        }
    }
}
