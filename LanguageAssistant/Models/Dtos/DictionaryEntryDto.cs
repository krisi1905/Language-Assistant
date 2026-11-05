
using System.Text.Json.Serialization;

namespace LanguageAssistant.Models.Dtos
{
    public class DictionaryEntryDto
    {
        [JsonPropertyName("word")]
        public string Word { get; set; } = "";

        [JsonPropertyName("phonetic")]
        public string? Phonetic { get; set; }

        [JsonPropertyName("meanings")]
        public List<MeaningDto> Meanings { get; set; } = new();
    }

    public class MeaningDto
    {
        [JsonPropertyName("partOfSpeech")]
        public string PartOfSpeech { get; set; } = "";

        [JsonPropertyName("definitions")]
        public List<DefinitionDto> Definitions { get; set; } = new();

        [JsonPropertyName("synonyms")]
        public List<string> Synonyms { get; set; } = new();
    }

    public class DefinitionDto
    {
        [JsonPropertyName("definition")]
        public string Definition { get; set; } = "";

        [JsonPropertyName("example")]
        public string? Example { get; set; }

        [JsonPropertyName("synonyms")]
        public List<string> Synonyms { get; set; } = new();
    }
}

