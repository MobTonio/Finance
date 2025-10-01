using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finance.EntityConfigurations
{
    public partial class TransactionConfiguration : IEntityTypeConfiguration<Transactions>
    {
        public void Configure(EntityTypeBuilder<Transactions> entity)
        {
            entity.HasKey(e => e.TransactionId);
            entity.ToTable(nameof(Transactions));
            entity.HasComment("Транзакции");
            entity.HasIndex(e => e.TransactionId, "IX_AOD_AODUid"); // ??

            entity.Property(e => e.TransactionId)
                .ValueGeneratedOnAdd()
                .HasComment("Идентификатор транзакции");

            entity.Property(e => e.WalletId)
                .HasComment("Идентификатор кошелька");

            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasComment("Дата");

            entity.Property(e => e.Amount)
                .HasComment("Сумма");

            entity.Property(e => e.Type)
                .HasConversion<string>() // Сохраняем как строку в БД
                .IsRequired()
                .HasMaxLength(10)
                .HasComment("Тип");

            entity.Property(e => e.Description)
                .HasMaxLength(512)
                .HasComment("Описание");

            entity.HasOne(d => d.Wallet)
                .WithMany(p => p.Transactions)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_Wallet");

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<Transactions> entity);
    }
}
