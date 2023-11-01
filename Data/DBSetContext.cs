using Job_By_SAP.Models;
using Job_By_SAP.WCM;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Read_xml.Models;

namespace Read_xml.Data
{
    public class DBSetContext : DbContext
    {
        public DbSet<TransTempGCP_WCM> TransTempGCP_WCMs { get; set; }
        public DBSetContext(DbContextOptions<DBSetContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransTempGCP_WCM>()
                .HasKey(m => m.ReceiptNo);
        }
    }
}
