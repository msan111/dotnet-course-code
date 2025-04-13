using Helloworld.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Helloworld.Data
{
    public class DataContextEF : DbContext
    {

        private readonly IConfiguration _config;
        public DataContextEF(IConfiguration config)
        {
            _config = config;
        }
        public DbSet<Computer>? Computer { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if(!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_config.GetConnectionString("DefaultConnection"),
                    options => options.EnableRetryOnFailure());
            }
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("TutorialAppSchema");

            modelBuilder.Entity<Computer>()
                .HasKey(c=> c.ComputerId);
                //.ToTable("Computer","TutorialAppSchema");
        }

    }
}