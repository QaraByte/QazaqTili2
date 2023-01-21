using Microsoft.EntityFrameworkCore;
using QazaqTili2.Models;

namespace QazaqTili2
{
    public class ApplicationContext
    {
        public DbSet<Word> Words => Set<Word>();
        public ApplicationContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=Qazaqtili;Trusted_Connection=True;");
        }
    }
}
