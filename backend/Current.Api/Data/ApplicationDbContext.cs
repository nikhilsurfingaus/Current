using Current.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Data;

// EF Core entry point — like a SQLAlchemy Session + model config combined
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
                .IsUnique(); // no duplicate emails
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(account => account.Id);

            entity.Property(account => account.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(account => account.AccountType)
                .HasConversion<string>() // store enum as "Everyday", "Savings", etc.
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(account => account.CurrentBalance)
                .HasPrecision(18, 2); // standard money precision

            entity.Property(account => account.Currency)
                .HasMaxLength(3)
                .IsRequired();

            // User 1 ──→ many Accounts
            entity.HasOne(account => account.User)
                .WithMany(user => user.Accounts)
                .HasForeignKey(account => account.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
