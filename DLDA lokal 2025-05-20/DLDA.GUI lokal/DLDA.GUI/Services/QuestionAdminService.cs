using DLDA.GUI.DTOs.Question;
using System.Net.Http.Json;

/// <summary>
/// Serviceklass för att hantera admin-relaterade API-anrop för frågor.
/// </summary>
public class QuestionAdminService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QuestionAdminService> _logger;

    /// <summary>
    /// Skapar en instans av QuestionAdminService.
    /// </summary>
    public QuestionAdminService(IHttpClientFactory factory, ILogger<QuestionAdminService> logger)
    {
        _httpClient = factory.CreateClient("DLDA");
        _logger = logger;
    }

    /// <summary>
    /// Hämtar alla frågor från API:t.
    /// </summary>
    public async Task<List<Question>> GetAllQuestionsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("Question");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Question>>() ?? new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hämtning av alla frågor.");
            return new();
        }
    }

    /// <summary>
    /// Hämtar en enskild fråga med ID.
    /// </summary>
    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"Question/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Question>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hämtning av fråga ID {Id}.", id);
            return null;
        }
    }

    /// <summary>
    /// Skickar in en ny fråga till API:t.
    /// </summary>
    public async Task<bool> CreateQuestionAsync(Question dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("Question", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid skapande av fråga.");
            return false;
        }
    }

    /// <summary>
    /// Uppdaterar en befintlig fråga i API:t.
    /// </summary>
    public async Task<bool> UpdateQuestionAsync(int id, Question dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"Question/{id}", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid uppdatering av fråga ID {Id}.", id);
            return false;
        }
    }

    /// <summary>
    /// Tar bort en fråga med angivet ID.
    /// </summary>
    public async Task<bool> DeleteQuestionAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"Question/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid borttagning av fråga ID {Id}.", id);
            return false;
        }
    }
}
