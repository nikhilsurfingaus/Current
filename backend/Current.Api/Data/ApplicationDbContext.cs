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
    }
}
