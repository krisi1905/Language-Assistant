
using LanguageAssistant.Data;
using LanguageAssistant.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LanguageAssistant.Controllers
{
    public class WordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var words = await _context.Words.ToListAsync();
            return View(words);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Word word)
        {
            if (ModelState.IsValid)
            {
                _context.Words.Add(word);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(word);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var word = await _context.Words.FindAsync(id);

            if (word == null)
            {
                return NotFound();
            }

            return View(word);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Word word)
        {
            if (id != word.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(word);
            }

            var existingWord =
                await _context.Words.FindAsync(id);

            if (existingWord == null)
            {
                return NotFound();
            }

            existingWord.Text = word.Text;
            existingWord.Translation = word.Translation;
            existingWord.Synonyms = word.Synonyms;
            existingWord.Example = word.Example;
            existingWord.DifficultyLevel = word.DifficultyLevel;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Думата е редактирана успешно!";

            return RedirectToAction(nameof(Index));
        }

       
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var word = await _context.Words.FindAsync(id);

            if (word == null)
            {
                return NotFound();
            }

            return View(word);
        }

        
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var word = await _context.Words.FindAsync(id);

            if (word == null)
            {
                return NotFound();
            }

            _context.Words.Remove(word);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Думата е изтрита успешно!";

            return RedirectToAction(nameof(Index));
        }

    }
}

