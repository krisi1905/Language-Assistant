
using LanguageAssistant.Models.Dtos;
using System.Text.Json;

namespace LanguageAssistant.Services
{
    public class DictionaryService
    {
        private readonly HttpClient _httpClient;

        public DictionaryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<DictionaryEntryDto?> GetWordAsync(
            string word)
        {
            string url =
                "https://api.dictionaryapi.dev/api/v2/entries/en/"
                + Uri.EscapeDataString(word.Trim());

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
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
            catch (HttpRequestException)
            {
                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}
