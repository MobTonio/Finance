using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Data.Contexts
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext() 
        { 
        }

        public AppDbContext(DbContextOptions options) 
            : base(options)
        {
        }

        public virtual DbSet<Transactions> Transactions { get; set; } = null!;
        public virtual DbSet<Wallet> Wallets { get; set; } = null!;

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    // Для PostgreSQL используйте UseNpgsql, а не UseSqlServer!
        //    optionsBuilder.UseNpgsql("Host=finance-server;Port=5432;Database=financeDB;Username=postgres;Password=postgres");
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EntityConfigurations.WalletConfiguration());
            modelBuilder.ApplyConfiguration(new EntityConfigurations.TransactionConfiguration());
            
            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
