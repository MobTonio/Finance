using Microsoft.EntityFrameworkCore;

namespace Finance.Contexts
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EntityConfigurations.WalletConfiguration());
            modelBuilder.ApplyConfiguration(new EntityConfigurations.TransactionConfiguration());
            
            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
