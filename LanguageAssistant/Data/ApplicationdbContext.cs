
using LanguageAssistant.Models;
using Microsoft.EntityFrameworkCore;

namespace LanguageAssistant.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Word> Words { get; set; }
    }
}

