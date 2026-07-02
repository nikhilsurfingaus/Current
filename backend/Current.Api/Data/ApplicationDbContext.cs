using Current.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);

            entity.Property(user => user.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(user => user.LastName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(user => user.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(user => user.Email)
                .IsUnique(); // One email per user
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(account => account.Id);

            entity.Property(account => account.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(account => account.AccountType)
                .HasConversion<string>() // Persist enum as string
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(account => account.CurrentBalance)
                .HasPrecision(18, 2); // Monetary precision

            entity.Property(account => account.Currency)
                .HasMaxLength(3)
                .IsRequired();

            // One user owns many accounts
            entity.HasOne(account => account.User)
                .WithMany(user => user.Accounts)
                .HasForeignKey(account => account.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Remove accounts when user is deleted
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(transaction => transaction.Id);

            entity.Property(transaction => transaction.Amount)
                .HasPrecision(18, 2);

            entity.Property(transaction => transaction.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(transaction => transaction.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.HasOne(transaction => transaction.FromAccount)
                .WithMany()
                .HasForeignKey(transaction => transaction.FromAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(transaction => transaction.ToAccount)
                .WithMany()
                .HasForeignKey(transaction => transaction.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LedgerEntry>(entity =>
        {
            entity.HasKey(ledgerEntry => ledgerEntry.Id);

            entity.Property(ledgerEntry => ledgerEntry.EntryType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(ledgerEntry => ledgerEntry.Amount)
                .HasPrecision(18, 2);

            entity.HasOne(ledgerEntry => ledgerEntry.Transaction)
                .WithMany(transaction => transaction.LedgerEntries)
                .HasForeignKey(ledgerEntry => ledgerEntry.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ledgerEntry => ledgerEntry.Account)
                .WithMany()
                .HasForeignKey(ledgerEntry => ledgerEntry.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
