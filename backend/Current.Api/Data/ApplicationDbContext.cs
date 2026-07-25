using Current.Api.Common.Enums;
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

    public DbSet<Goal> Goals => Set<Goal>();

    public DbSet<GoalContribution> GoalContributions => Set<GoalContribution>();

    public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

    public DbSet<Contact> Contacts => Set<Contact>();

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<Loan> Loans => Set<Loan>();

    public DbSet<LoanRepayment> LoanRepayments => Set<LoanRepayment>();

    public DbSet<Notification> Notifications => Set<Notification>();

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

            entity.Property(user => user.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(user => user.IsEmailVerified)
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(user => user.EmailVerificationCodeHash)
                .HasMaxLength(500);

            entity.Property(user => user.Role)
                .HasConversion<string>()
                .HasMaxLength(50)
                .HasDefaultValue(UserRole.User)
                .IsRequired();

            entity.Property(user => user.ThemePreference)
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasDefaultValue(ThemePreference.System)
                .IsRequired();

            entity.Property(user => user.PreferredCurrency)
                .HasMaxLength(3)
                .HasDefaultValue("AUD")
                .IsRequired();

            entity.Property(user => user.Timezone)
                .HasMaxLength(100)
                .HasDefaultValue("Australia/Sydney")
                .IsRequired();

            entity.Property(user => user.Locale)
                .HasMaxLength(20)
                .HasDefaultValue("en-AU")
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

            entity.Property(transaction => transaction.Category)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(transaction => transaction.Merchant)
                .HasMaxLength(200);

            entity.Property(transaction => transaction.Reference)
                .HasMaxLength(100);

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

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(goal => goal.Id);

            entity.Property(goal => goal.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(goal => goal.Description)
                .HasMaxLength(500);

            entity.Property(goal => goal.TargetAmount)
                .HasPrecision(18, 2);

            entity.Property(goal => goal.CurrentAmount)
                .HasPrecision(18, 2);

            entity.Property(goal => goal.Currency)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(goal => goal.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(goal => goal.IconKey)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasOne(goal => goal.User)
                .WithMany(user => user.Goals)
                .HasForeignKey(goal => goal.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(goal => goal.SourceAccount)
                .WithMany()
                .HasForeignKey(goal => goal.SourceAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(goal => goal.GoalAccount)
                .WithMany()
                .HasForeignKey(goal => goal.GoalAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IdempotencyKey>(entity =>
        {
            entity.HasKey(idempotencyKey => idempotencyKey.Id);

            entity.Property(idempotencyKey => idempotencyKey.Key)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(idempotencyKey => idempotencyKey.RequestHash)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(idempotencyKey => idempotencyKey.ResponseJson)
                .IsRequired();

            entity.HasIndex(idempotencyKey => new { idempotencyKey.UserId, idempotencyKey.Key })
                .IsUnique();

            entity.HasOne(idempotencyKey => idempotencyKey.User)
                .WithMany()
                .HasForeignKey(idempotencyKey => idempotencyKey.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(contact => contact.Id);

            entity.Property(contact => contact.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(contact => contact.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasIndex(contact => new { contact.UserId, contact.Email })
                .IsUnique();

            entity.HasOne(contact => contact.User)
                .WithMany()
                .HasForeignKey(contact => contact.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GoalContribution>(entity =>
        {
            entity.HasKey(contribution => contribution.Id);

            entity.Property(contribution => contribution.Amount)
                .HasPrecision(18, 2);

            entity.Property(contribution => contribution.ContributionType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(contribution => contribution.Note)
                .HasMaxLength(500);

            entity.HasOne(contribution => contribution.Goal)
                .WithMany(goal => goal.Contributions)
                .HasForeignKey(contribution => contribution.GoalId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(contribution => contribution.Transaction)
                .WithMany()
                .HasForeignKey(contribution => contribution.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(branch => branch.Id);

            entity.Property(branch => branch.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(branch => branch.Code)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(branch => branch.Code)
                .IsUnique();

            entity.HasOne(branch => branch.TreasuryAccount)
                .WithMany()
                .HasForeignKey(branch => branch.TreasuryAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(loan => loan.Id);

            entity.Property(loan => loan.Principal)
                .HasPrecision(18, 2);

            entity.Property(loan => loan.OutstandingPrincipal)
                .HasPrecision(18, 2);

            entity.Property(loan => loan.InterestRatePercent)
                .HasPrecision(8, 4);

            entity.Property(loan => loan.MonthlyPayment)
                .HasPrecision(18, 2);

            entity.Property(loan => loan.Currency)
                .HasMaxLength(3)
                .IsRequired();

            entity.Property(loan => loan.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(loan => loan.Purpose)
                .HasMaxLength(500);

            entity.Property(loan => loan.RejectionReason)
                .HasMaxLength(500);

            entity.HasOne(loan => loan.User)
                .WithMany(user => user.Loans)
                .HasForeignKey(loan => loan.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(loan => loan.Branch)
                .WithMany()
                .HasForeignKey(loan => loan.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(loan => loan.FundedAccount)
                .WithMany()
                .HasForeignKey(loan => loan.FundedAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(loan => loan.DisbursementTransaction)
                .WithMany()
                .HasForeignKey(loan => loan.DisbursementTransactionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<LoanRepayment>(entity =>
        {
            entity.HasKey(repayment => repayment.Id);

            entity.Property(repayment => repayment.Amount)
                .HasPrecision(18, 2);

            entity.Property(repayment => repayment.PrincipalPortion)
                .HasPrecision(18, 2);

            entity.Property(repayment => repayment.InterestPortion)
                .HasPrecision(18, 2);

            entity.HasOne(repayment => repayment.Loan)
                .WithMany(loan => loan.Repayments)
                .HasForeignKey(repayment => repayment.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(repayment => repayment.Transaction)
                .WithMany()
                .HasForeignKey(repayment => repayment.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(notification => notification.Id);

            entity.Property(notification => notification.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(notification => notification.Body)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(notification => notification.NotificationType)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(notification => new { notification.UserId, notification.CreatedAt });

            entity.HasOne(notification => notification.User)
                .WithMany()
                .HasForeignKey(notification => notification.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
