using BluePosVoucher.Models;
using Job_By_SAP.Models;
using Job_By_SAP.WCM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Read_xml.Models;

namespace Read_xml.Data
{
    public class DBSetContext : DbContext
    {
        private string _connectionString;

        public DBSetContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);

        }
        public DbSet<INB_VoucherToSAP> INB_VoucherToSAP { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<INB_VoucherToSAP>()
                .HasKey(m => m.SerialNo);
        }
    }
}
