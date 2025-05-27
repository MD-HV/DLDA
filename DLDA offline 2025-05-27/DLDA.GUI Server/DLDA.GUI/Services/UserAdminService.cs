using DLDA.GUI.DTOs.User;
using System.Net.Http.Json;

namespace DLDA.GUI.Services
{
    /// <summary>
    /// Serviceklass för att hantera API-anrop som rör admin-hantering av användare.
    /// </summary>
    public class UserAdminService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserAdminService> _logger;

        /// <summary>
        /// Skapar en instans av UserAdminService.
        /// </summary>
        public UserAdminService(IHttpClientFactory factory, ILogger<UserAdminService> logger)
        {
            _httpClient = factory.CreateClient("DLDA");
            _logger = logger;
        }

        /// <summary>
        /// Hämtar alla användare från API:t.
        /// </summary>
        public async Task<List<UserDto>> GetAllAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<UserDto>>("User") ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av användarlista.");
                return new();
            }
        }

        /// <summary>
        /// Hämtar en användare baserat på ID.
        /// </summary>
        public async Task<UserDto?> GetByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserDto>($"User/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid hämtning av användare ID: {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Skapar en ny användare.
        /// </summary>
        public async Task<bool> CreateAsync(UserDto user)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("User", user);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid skapande av användare.");
                return false;
            }
        }

        /// <summary>
        /// Uppdaterar en befintlig användare.
        /// </summary>
        public async Task<bool> UpdateAsync(int id, UserDto user)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"User/{id}", user);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid uppdatering av användare ID: {Id}", id);
                return false;
            }
        }

        /// <summary>
        /// Tar bort en användare.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"User/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fel vid borttagning av användare ID: {Id}", id);
                return false;
            }
        }
    }
}
