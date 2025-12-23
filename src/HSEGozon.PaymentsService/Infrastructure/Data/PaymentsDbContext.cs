using Microsoft.EntityFrameworkCore;
using HSEGozon.PaymentsService.Domain.Entities;

namespace HSEGozon.PaymentsService.Infrastructure.Data;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.Balance).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Account)
                .WithMany()
                .HasForeignKey(e => e.AccountId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.OrderId, e.MessageId }).IsUnique();
        });

        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MessageId).IsUnique();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}

