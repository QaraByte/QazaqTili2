using Microsoft.EntityFrameworkCore;
using QazaqTili2.Models;

namespace QazaqTili2
{
    public class ApplicationContext:DbContext
    {

        //public ApplicationContext() => Database.EnsureCreated();

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            //optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=Qazaqtili;Trusted_Connection=True;");
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=Qazaqtili;Trusted_Connection=True;");
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Word>()
                .HasOne(w => w.WordTypes)
                .WithMany(wt => wt.Words)
                .HasForeignKey(w => w.WordTypeId);
        }

        public DbSet<Word> Words => Set<Word>();
    }
}
