using Microsoft.EntityFrameworkCore;
using MutualFundStats.Models;
namespace MutualFundStats.Data
{
    public class MutualFundDataContext : DbContext
    {
        public MutualFundDataContext(DbContextOptions<MutualFundDataContext> options) : base(options) 
        {
            
        }
        public MutualFundDataContext() { }
        public DbSet<Scheme> Schemes { get; set; }
        public DbSet<MutualFundData> MutualFundDatas {  get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False;Database=MutualFund");
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Scheme>().HasKey(x => x.SchemeId);

            modelBuilder.Entity<MutualFundData>().HasKey(x => x.StatId);

            modelBuilder.Entity<MutualFundData>()
                .HasOne(ms =>ms.Scheme)
                .WithMany(ms => ms.MutualFundDatas)
                .HasForeignKey(ms => ms.SchemeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
