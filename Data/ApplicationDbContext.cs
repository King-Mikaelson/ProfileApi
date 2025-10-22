using Microsoft.EntityFrameworkCore;
using StringAnalyzer.Models;
using StringAnalyzer.Services;

namespace StringAnalyzer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // This creates the "AnalyzedStrings" table
        public DbSet<AnalyzedString> AnalyzedStrings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<StringAnalyzerService>();

            // Configure the StringEntity
            modelBuilder.Entity<AnalyzedString>(entity =>
            {
                // Set Id as primary key
                entity.HasKey(e => e.Id);

                // Create index on Value for faster lookups
                entity.HasIndex(e => e.Value);

                // Set default value for CreatedAt
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}