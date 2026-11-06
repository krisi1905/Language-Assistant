
using LanguageAssistant.Models.Dtos;
using System.Text.Json;

namespace LanguageAssistant.Services
{
    public class DictionaryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DictionaryService> _logger;

        public DictionaryService(
            HttpClient httpClient,
            ILogger<DictionaryService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<DictionaryEntryDto?> GetWordAsync(string word)
        {
            string url =
                "https://api.dictionaryapi.dev/api/v2/entries/en/"
                + Uri.EscapeDataString(word.Trim());

            try
            {

                using var request = new HttpRequestMessage(
                    HttpMethod.Get, url);

                request.Headers.UserAgent.ParseAdd(
                    "LanguageAssistant/1.0");

                request.Headers.Accept.ParseAdd(
                    "application/json");

                using var response = await _httpClient.SendAsync(request);


                _logger.LogInformation(
                    "Dictionary API status: {StatusCode}",
                    (int)response.StatusCode);


                if (!response.IsSuccessStatusCode)
                {
                    var errorBody =
                        await response.Content.ReadAsStringAsync();

                    var shortError = errorBody.Length > 200
                        ? errorBody.Substring(0, 200)
                        : errorBody;

                    _logger.LogWarning(
                        "Dictionary API error {StatusCode}. " +
                        "Content-Type: {ContentType}. " +
                        "Response: {Response}",
                        (int)response.StatusCode,
                        response.Content.Headers.ContentType?.MediaType,
                        shortError);

                    return null;
                }


                var json = await response.Content.ReadAsStringAsync();

                var results =
                    JsonSerializer.Deserialize<List<DictionaryEntryDto>>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                return results?.FirstOrDefault();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    "Dictionary API network error: {Message}",
                    ex.Message);
                return null;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    "Dictionary API timeout: {Message}",
                    ex.Message);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    "Dictionary API JSON error: {Message}",
                    ex.Message);
                return null;
            }
        }
    }
}
