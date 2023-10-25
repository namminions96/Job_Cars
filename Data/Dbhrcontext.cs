using Job_By_SAP.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Read_xml.Models;

namespace Read_xml.Data
{
    public class Dbhrcontext : DbContext
    {
        public DbSet<HR_Dashboard> HR_Dashboards { get; set; }
        public DbSet<HR_Terninate> HR_Terninates { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<Error> Errors { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            string connectString = configuration["serverConfig"];
            optionsBuilder.UseSqlServer(connectString);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Error>()
                .HasKey(m => m.messages); 
        }

    }
}
