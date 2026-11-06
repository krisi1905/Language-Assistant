
using LanguageAssistant.Data;
using LanguageAssistant.Models;
using LanguageAssistant.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LanguageAssistant.Controllers
{
    public class AIController : Controller
    {
        private readonly GeminiService _geminiService;
        private readonly ApplicationDbContext _context;

        public AIController(
            GeminiService geminiService,
            ApplicationDbContext context)
        {
            _geminiService = geminiService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analyze(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                ViewBag.Error = "Моля, въведи дума.";
                return View("Index");
            }

            var result = await _geminiService
                .GenerateWordAsync(word.Trim());

            if (result == null)
            {
                ViewBag.Error =
                    "AI не успя да анализира думата.";
                return View("Index");
            }

            return View("Index", result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(
            [Bind("Text,Translation,Synonyms,Example,DifficultyLevel")]
            Word word)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Провери въведените данни.";
                return View("Index", word);
            }

            word.Text = word.Text.Trim();
            word.Translation = word.Translation.Trim();

            bool exists = await _context.Words
                .AnyAsync(w => w.Text == word.Text);

            if (exists)
            {
                ViewBag.Error =
                    "Тази дума вече е записана.";
                return View("Index", word);
            }

            word.Id = 0;

            word.DateAdded = DateTime.UtcNow;


            _context.Words.Add(word);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Думата е записана успешно чрез AI!";

            return RedirectToAction("Index", "Words");
        }
    }
}
