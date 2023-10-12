using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BluePosVoucher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Job_By_SAP.Models;

namespace BluePosVoucher.Data
{
    public class DBINBOUND : DbContext
    {
        public DbSet<ConfigConnections> ConfigConnections { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<OrderExpToGCP> OrderExpToGCPs { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            string connectString = configuration["INBOUND"];
            optionsBuilder.UseSqlServer(connectString);

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderExpToGCP>().HasNoKey();
        }

    }
}
