using DLDA.GUI.DTOs.Authentication;
using System.Net.Http.Json;

/// <summary>
/// Service som hanterar kontorelaterad kommunikation med DLDA.API, t.ex. inloggning.
/// </summary>
public class AccountService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountService> _logger;

    /// <summary>
    /// Skapar en ny instans av AccountService.
    /// </summary>
    /// <param name="factory">HttpClientFactory som används för att hämta klienten konfigurerad för DLDA.API.</param>
    /// <param name="logger">Logger för loggning av fel och varningar.</param>
    public AccountService(IHttpClientFactory factory, ILogger<AccountService> logger)
    {
        _httpClient = factory.CreateClient("DLDA");
        _logger = logger;
    }

    /// <summary>
    /// Försöker logga in en användare via DLDA.API.
    /// </summary>
    /// <param name="login">DTO som innehåller användarnamn och lösenord.</param>
    /// <returns>AuthResponseDto om inloggning lyckas, annars null.</returns>
    public async Task<AuthResponseDto?> LoginAsync(LoginDto login)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/login", login);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Inloggning misslyckades. Status: {Status}", response.StatusCode);
                return null;
            }

            var user = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (user == null)
                _logger.LogError("Inloggningssvaret kunde inte deserialiseras.");

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid API-anrop under inloggning.");
            return null;
        }
    }
}
