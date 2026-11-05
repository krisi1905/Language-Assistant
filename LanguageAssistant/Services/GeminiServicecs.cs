
using System.Net.Http.Json;
using System.Text.Json;
using LanguageAssistant.Models;

namespace LanguageAssistant.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeminiService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> AnalyzeWordAsync(string word)
        {
            string? apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return "Липсва Gemini API ключ.";
            }

            string prompt = $"""
                Ти си интелигентен езиков помощник.

                Анализирай английската дума: {word}

                Отговори на български и включи:
                1. Превод на български.
                2. Три английски синонима.
                3. Примерно изречение на английски.
                4. Превод на примерното изречение.
                5. Приблизително ниво на трудност
                   според CEFR: A1, A2, B1, B2, C1 или C2.

                Отговори кратко и ясно.
                """;

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            string url =
                "https://generativelanguage.googleapis.com/" +
                "v1beta/models/gemini-3.5-flash-lite:generateContent";

            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Post, url);

                request.Headers.Add("x-goog-api-key", apiKey);
                request.Content = JsonContent.Create(requestBody);

                using var response =
                    await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return $"Грешка от Gemini API: " +
                           $"{(int)response.StatusCode}";
                }

                using var stream =
                    await response.Content.ReadAsStreamAsync();

                using var document =
                    await JsonDocument.ParseAsync(stream);

                var candidates = document.RootElement
                    .GetProperty("candidates");

                var parts = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts");

                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text))
                    {
                        return text.GetString() ?? "";
                    }
                }

                return "Няма текстов отговор от AI.";
            }
            catch (Exception ex) when (
                ex is HttpRequestException ||
                ex is TaskCanceledException ||
                ex is JsonException ||
                ex is KeyNotFoundException ||
                ex is IndexOutOfRangeException)
            {
                return "Неуспешна заявка към Gemini API.";
            }
        }

        public async Task<Word?> GenerateWordAsync(string word)
        {
            string? apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
                return null;

            string prompt = $"""
        Анализирай английската дума "{word}".

        Върни:
        - text: английската дума
        - translation: превод на български
        - synonyms: 3 английски синонима,
          разделени със запетая
        - example: примерно изречение на английски
        - difficultyLevel: приблизително CEFR ниво
          A1, A2, B1, B2, C1 или C2

        Използвай най-разпространеното значение.
        Ако няма подходящи синоними, върни празен низ.
        """;

            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                parts = new[] { new { text = prompt } }
            }
        },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    responseJsonSchema = new
                    {
                        type = "object",
                        properties = new
                        {
                            text = new { type = "string" },
                            translation = new { type = "string" },
                            synonyms = new { type = "string" },
                            example = new { type = "string" },
                            difficultyLevel = new
                            {
                                type = "string",
                                @enum = new[]
                                {
                            "A1", "A2", "B1",
                            "B2", "C1", "C2"
                        }
                            }
                        },
                        required = new[]
                        {
                    "text", "translation", "synonyms",
                    "example", "difficultyLevel"
                }
                    }
                }
            };

            string url =
                "https://generativelanguage.googleapis.com/" +
                "v1beta/models/gemini-3.5-flash-lite:generateContent";

            try
            {
                using var request =
                    new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Add("x-goog-api-key", apiKey);
                request.Content = JsonContent.Create(requestBody);

                using var response =
                    await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return null;

                using var stream =
                    await response.Content.ReadAsStreamAsync();

                using var document =
                    await JsonDocument.ParseAsync(stream);

                string json = document.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                var result = JsonSerializer.Deserialize<Word>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (result == null ||
                    string.IsNullOrWhiteSpace(result.Text) ||
                    string.IsNullOrWhiteSpace(result.Translation))
                {
                    return null;
                }

                return result;
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
            catch (KeyNotFoundException)
            {
                return null;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

    }
}

