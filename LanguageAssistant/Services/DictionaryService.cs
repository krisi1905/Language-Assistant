
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
            if (string.IsNullOrWhiteSpace(word))
                return null;

            word = word.Trim();

            // Първо опитваме основния речник.
            var result = await GetFromDictionaryApiAsync(word);

            if (result != null)
                return result;

            // Ако е недостъпен, използваме Datamuse.
            _logger.LogInformation(
                "Trying Datamuse fallback for {Word}", word);

            return await GetFromDatamuseAsync(word);
        }

        private async Task<DictionaryEntryDto?> GetFromDictionaryApiAsync(
            string word)
        {
            var url =
                "https://api.dictionaryapi.dev/api/v2/entries/en/"
                + Uri.EscapeDataString(word);

            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Get, url);

                request.Headers.UserAgent.ParseAdd(
                    "LanguageAssistant/1.0");

                request.Headers.Accept.ParseAdd("application/json");

                using var response =
                    await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Dictionary API status: {Status}",
                        (int)response.StatusCode);

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
            catch (Exception ex) when (
                ex is HttpRequestException ||
                ex is TaskCanceledException ||
                ex is JsonException)
            {
                _logger.LogWarning(
                    "Dictionary API failed: {Message}",
                    ex.Message);

                return null;
            }
        }

        private async Task<DictionaryEntryDto?> GetFromDatamuseAsync(
            string word)
        {
            var encoded = Uri.EscapeDataString(word);

            var url =
                $"https://api.datamuse.com/words?sp={encoded}&md=dpr&max=10&ipa=1";

            try
            {
                using var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Datamuse returned status {Status}",
                        (int)response.StatusCode);

                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();

                using var document = JsonDocument.Parse(json);

                JsonElement? match = null;

                foreach (var item in document.RootElement.EnumerateArray())
                {
                    if (item.TryGetProperty("word", out var name) &&
                        string.Equals(
                            name.GetString(),
                            word,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        match = item;
                        break;
                    }
                }

                if (match == null)
                    return null;

                var entry = new DictionaryEntryDto
                {
                    Word = word
                };

                var itemData = match.Value;

                // Произношение, ако е налично.
                if (itemData.TryGetProperty("tags", out var tags) &&
                    tags.ValueKind == JsonValueKind.Array)
                {
                    foreach (var tag in tags.EnumerateArray())
                    {
                        var value = tag.GetString();

                        if (value != null &&
                            value.StartsWith("pron:"))
                        {
                            entry.Phonetic = value.Substring(5).Trim();
                            break;
                        }
                    }
                }

                // Определения и части на речта.
                var meanings = new Dictionary<string, MeaningDto>();

                if (itemData.TryGetProperty("defs", out var defs) &&
                    defs.ValueKind == JsonValueKind.Array)
                {
                    foreach (var definition in defs.EnumerateArray())
                    {
                        var raw = definition.GetString();

                        if (string.IsNullOrWhiteSpace(raw))
                            continue;

                        var parts = raw.Split('\t', 2);

                        string partOfSpeech = "other";
                        string definitionText = raw;

                        if (parts.Length == 2)
                        {
                            definitionText = parts[1];

                            partOfSpeech = parts[0] switch
                            {
                                "n" => "noun",
                                "v" => "verb",
                                "adj" => "adjective",
                                "adv" => "adverb",
                                _ => parts[0]
                            };
                        }

                        if (!meanings.TryGetValue(
                            partOfSpeech, out var meaning))
                        {
                            meaning = new MeaningDto
                            {
                                PartOfSpeech = partOfSpeech
                            };

                            meanings[partOfSpeech] = meaning;
                        }

                        meaning.Definitions.Add(new DefinitionDto
                        {
                            Definition = definitionText
                        });
                    }
                }

                entry.Meanings = meanings.Values.ToList();

                // Извличане на синоними от Datamuse.
                var synonymsUrl =
                    $"https://api.datamuse.com/words?rel_syn={encoded}&max=10";

                try
                {
                    using var synResponse =
                        await _httpClient.GetAsync(synonymsUrl);

                    if (synResponse.IsSuccessStatusCode)
                    {
                        var synonymsJson =
                            await synResponse.Content.ReadAsStringAsync();

                        using var synDocument =
                            JsonDocument.Parse(synonymsJson);

                        var synonyms = new List<string>();

                        foreach (var syn in synDocument.RootElement.EnumerateArray())
                        {
                            if (syn.TryGetProperty("word", out var name))
                            {
                                var synonym = name.GetString();

                                if (!string.IsNullOrWhiteSpace(synonym))
                                    synonyms.Add(synonym);
                            }
                        }

                        if (synonyms.Count > 0)
                        {
                            if (entry.Meanings.Count == 0)
                            {
                                entry.Meanings.Add(new MeaningDto
                                {
                                    PartOfSpeech = "other"
                                });
                            }

                            entry.Meanings[0].Synonyms = synonyms;
                        }
                    }
                }
                catch (Exception ex) when (
                    ex is HttpRequestException ||
                    ex is TaskCanceledException ||
                    ex is JsonException)
                {
                    _logger.LogWarning(
                        "Datamuse synonyms failed: {Message}",
                        ex.Message);
                }

                // Показваме резултат само ако има данни.
                if (entry.Meanings.Count == 0)
                    return null;

                _logger.LogInformation(
                    "Datamuse fallback succeeded for {Word}", word);

                return entry;
            }
            catch (Exception ex) when (
                ex is HttpRequestException ||
                ex is TaskCanceledException ||
                ex is JsonException)
            {
                _logger.LogWarning(
                    "Datamuse fallback failed: {Message}",
                    ex.Message);

                return null;
            }
        }
    }
}
