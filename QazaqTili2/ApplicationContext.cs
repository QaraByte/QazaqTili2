﻿using Microsoft.EntityFrameworkCore;
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

            modelBuilder.Entity<YoutubeLinks>()
                .HasOne(w => w.Words)
                .WithMany(wt => wt.YoutubeLinks)
                .HasForeignKey(w => w.WordId);

            modelBuilder.Entity<MainIndex>().Property(p => p.Id)
            .HasColumnType("int");
        }

        public DbSet<Word> Words => Set<Word>();
        public DbSet<WordTypes> WordTypes => Set<WordTypes>();
        public DbSet<YoutubeLinks> YoutubeLinks => Set<YoutubeLinks>();
        public DbSet<FileModel> Files { get; set; }
        public DbSet<ImageLinks> ImageLinks { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
