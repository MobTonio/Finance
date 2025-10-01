using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace Finance.EntityConfigurations
{
    public partial class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> entity)
        {
            entity.HasKey(e => e.Id);
            entity.ToTable(nameof(Wallet));
            entity.HasComment("Кошелек");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasComment("Идентификатор");

            entity.Property(e => e.Name)
                .HasMaxLength(512)
                .HasComment("Название");

            entity.Property(e => e.Currency)
                .HasMaxLength(512)
                .HasComment("Валюта");

            entity.Property(e => e.InitialBalance)
               .HasComment("Начальный баланс");

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<Wallet> entity);
    }
}
