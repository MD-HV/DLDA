using DLDA.GUI.DTOs.Assessment;
using DLDA.GUI.DTOs.User;
using System.Net.Http.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Serviceklass för att hantera personalens åtkomst till bedömningar och patientdata.
    /// </summary>
    public class StaffAssessmentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StaffAssessmentService> _logger;

        public StaffAssessmentService(IHttpClientFactory factory, ILogger<StaffAssessmentService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Hämtar alla patienter med senaste bedömning.
        /// </summary>
        public async Task<List<PatientWithLatestAssessmentDto>> GetPatientsWithLatestAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<PatientWithLatestAssessmentDto>>("User/with-latest-assessment") ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av patientlista.");
                return new();
            }
        }

        /// <summary>
        /// Hämtar användarens namn.
        /// </summary>
        public async Task<string> GetUsernameAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"User/{userId}");
                if (!response.IsSuccessStatusCode) return "Okänt namn";

                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                return user?.Username ?? "Okänt namn";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av användare {UserId}", userId);
                return "Okänt namn";
            }
        }

        /// <summary>
        /// Hämtar alla bedömningar för en specifik patient.
        /// </summary>
        public async Task<List<AssessmentDto>> GetAssessmentsForUserAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Assessment/user/{userId}");
                if (!response.IsSuccessStatusCode) return new();

                return await response.Content.ReadFromJsonAsync<List<AssessmentDto>>() ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av bedömningar för användare {UserId}", userId);
                return new();
            }
        }

        /// <summary>
        /// Skapar en ny bedömning för en patient.
        /// </summary>
        public async Task<bool> CreateAssessmentAsync(int userId)
        {
            var dto = new AssessmentDto
            {
                UserId = userId,
                ScaleType = "Numerisk",
                IsComplete = false
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("Assessment", dto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid skapande av ny bedömning.");
                return false;
            }
        }

        /// <summary>
        /// Hämtar en specifik bedömning.
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
        /// Raderar en bedömning baserat på ID.
        /// </summary>
        public async Task<bool> DeleteAssessmentAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Assessment/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid DeleteAssessmentAsync({Id})", id);
                return false;
            }
        }

        /// <summary>
        /// Hämtar  användare baserat på söksträng.
        /// </summary>
        public async Task<List<UserDto>> SearchPatientsAsync(string? search)
        {
            try
            {
                var endpoint = string.IsNullOrWhiteSpace(search)
                    ? "user/patients"
                    : $"user/patients?search={Uri.EscapeDataString(search)}";

                var response = await _httpClient.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                    return new List<UserDto>();

                var result = await response.Content.ReadFromJsonAsync<List<UserDto>>();
                return result ?? new List<UserDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid sökning av patienter");
                return new List<UserDto>();
            }
        }

        /// <summary>
        /// Hämtar en lista av patienter tillsammans med deras senaste bedömning,
        /// med möjlighet att filtrera på sökord, pågående status och datumintervall.
        /// </summary>
        public async Task<List<PatientWithAssessmentStatusDto>> GetFilteredPatientsAsync(
            string? search, bool? ongoing, bool? notOngoing, string? recent)
        {
            try
            {
                var queryParams = new List<string>();

                if (!string.IsNullOrWhiteSpace(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");

                if (ongoing == true)
                    queryParams.Add("ongoing=true");

                if (notOngoing == true)
                    queryParams.Add("notOngoing=true");

                if (!string.IsNullOrWhiteSpace(recent))
                    queryParams.Add($"recent={Uri.EscapeDataString(recent)}");

                var query = queryParams.Any()
                    ? "user/with-latest-assessment?" + string.Join("&", queryParams)
                    : "user/with-latest-assessment";

                var response = await _httpClient.GetAsync(query);
                if (!response.IsSuccessStatusCode)
                    return new List<PatientWithAssessmentStatusDto>();

                var result = await response.Content.ReadFromJsonAsync<List<PatientWithAssessmentStatusDto>>();
                return result ?? new List<PatientWithAssessmentStatusDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av filtrerad patientlista.");
                return new List<PatientWithAssessmentStatusDto>();
            }
        }

    }
}
