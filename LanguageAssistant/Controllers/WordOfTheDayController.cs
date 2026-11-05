
using LanguageAssistant.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LanguageAssistant.Controllers
{
    public class WordOfTheDayController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WordOfTheDayController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var words = await _context.Words
                .OrderBy(w => w.Id)
                .ToListAsync();

            if (words.Count == 0)
            {
                return View(null);
            }

            int dayNumber = DateTime.Today.DayOfYear
                + DateTime.Today.Year * 366;

            int index = dayNumber % words.Count;

            var wordOfTheDay = words[index];

            return View(wordOfTheDay);
        }
    }
}
 

