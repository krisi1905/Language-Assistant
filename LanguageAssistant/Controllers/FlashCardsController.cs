using LanguageAssistant.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LanguageAssistant.Controllers
{
    public class FlashcardsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FlashcardsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var words = await _context.Words
                .AsNoTracking()
                .OrderBy(w => w.Id)
                .ToListAsync();

            return View(words);
        }
    }
}
