
using LanguageAssistant.Data;
using LanguageAssistant.Models;
using LanguageAssistant.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LanguageAssistant.Controllers
{
    public class DictionaryController : Controller
    {
        private readonly DictionaryService _dictionaryService;
        private readonly ApplicationDbContext _context;

        public DictionaryController(
            DictionaryService dictionaryService,
            ApplicationDbContext context)
        {
            _dictionaryService = dictionaryService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                ViewBag.Error = "Моля, въведете дума.";
                return View("Index");
            }

            var result = await _dictionaryService.GetWordAsync(word);

            if (result == null)
            {
                ViewBag.Error = "Няма информация за тази дума.";
                return View("Index");
            }

            return View("Index", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(
        string text,
        string translation,
        string? synonyms,
        string? example,
        string? difficultyLevel)
        {
            if (string.IsNullOrWhiteSpace(text) ||
                string.IsNullOrWhiteSpace(translation))
            {
                TempData["Error"] =
                    "Думата и преводът са задължителни.";

                return RedirectToAction(nameof(Index));
            }

            text = text.Trim();
            translation = translation.Trim();

            bool exists = await _context.Words
                .AnyAsync(w => w.Text == text);

            if (exists)
            {
                TempData["Error"] =
                    "Тази дума вече е записана.";

                return RedirectToAction(nameof(Index));
            }

            var word = new Word
            {
                Text = text,
                Translation = translation,
                Synonyms = synonyms,
                Example = example,
                DifficultyLevel = difficultyLevel,
                DateAdded = DateTime.UtcNow
            };

            _context.Words.Add(word);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Думата е записана успешно!";

            return RedirectToAction("Index", "Words");
        }
    }
}
