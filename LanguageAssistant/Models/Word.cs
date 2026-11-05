
using System;
using System.ComponentModel.DataAnnotations;

namespace LanguageAssistant.Models
{
    public class Word
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        [Required]
        public string Translation { get; set; } = string.Empty;

        public string? Synonyms { get; set; }

        public string? Example { get; set; }

        public string? DifficultyLevel { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    }
}

