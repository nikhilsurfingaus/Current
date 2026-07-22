using Current.Api.Common.Constants;
using Current.Api.Common.Enums;
using Current.Api.Configuration;
using Current.Api.Data;
using Current.Api.DTOs.Loans;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Current.Api.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Current.Api.Services;

public class LoanService : ILoanService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDisbursementService _disbursementService;
    private readonly BranchOptions _branchOptions;

    public LoanService(
        ApplicationDbContext dbContext,
        IDisbursementService disbursementService,
        IOptions<BranchOptions> branchOptions)
    {
        _dbContext = dbContext;
        _disbursementService = disbursementService;
        _branchOptions = branchOptions.Value;
    }

    public async Task<IReadOnlyList<LoanResponse>> GetUserLoansAsync(Guid currentUserId)
    {
        var loans = await _dbContext.Loans
            .AsNoTracking()
            .Where(loan => loan.UserId == currentUserId)
            .OrderByDescending(loan => loan.CreatedAt)
            .ToListAsync();

        return loans.Select(loan => loan.ToResponse()).ToList();
    }

    public async Task<LoanResponse?> GetUserLoanByIdAsync(Guid loanId, Guid currentUserId)
    {
        var loan = await FindOwnedLoanAsync(loanId, currentUserId, asNoTracking: true);
        return loan?.ToResponse();
    }

    public async Task<LoanLimitsResponse> GetUserLoanLimitsAsync(Guid currentUserId)
    {
        var branch = await _disbursementService.GetDefaultBranchAsync();
        var loanCurrency = branch.TreasuryAccount.Currency;
        var totalHoldings = await GetUserHoldingsAsync(currentUserId, loanCurrency);
        var tier = ResolveLoanLimitTier(totalHoldings);
        var openLoanExposure = await GetOpenLoanExposureAsync(currentUserId);

        var maxSingleLoan = Math.Min(tier.MaxSingleLoan, _branchOptions.MaxLoanAmount);
        var maxTotalOutstanding = tier.MaxTotalOutstanding;
        var maxOpenLoans = Math.Min(tier.MaxOpenLoans, _branchOptions.MaxActiveLoansPerUser);
        var availableBorrowingCapacity = Math.Max(0, maxTotalOutstanding - openLoanExposure.TotalExposure);

        return new LoanLimitsResponse
        {
            Currency = loanCurrency,
            TotalHoldings = totalHoldings,
            TierLabel = tier.Label,
            MaxSingleLoan = maxSingleLoan,
            MaxTotalOutstanding = maxTotalOutstanding,
            MaxOpenLoans = maxOpenLoans,
            OpenLoanCount = openLoanExposure.OpenLoanCount,
            CurrentOutstandingExposure = openLoanExposure.TotalExposure,
            AvailableBorrowingCapacity = availableBorrowingCapacity
        };
    }

    public async Task<LoanResponse> CreateLoanRequestAsync(CreateLoanRequest request, Guid currentUserId)
    {
        var branch = await _disbursementService.GetDefaultBranchAsync();
        var fundedAccount = request.FundedAccountId.HasValue
            ? await ResolveOwnedFundedAccountAsync(request.FundedAccountId.Value, currentUserId)
            : await ResolveDefaultFundedAccountAsync(currentUserId);

        if (!string.Equals(fundedAccount.Currency, branch.TreasuryAccount.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Loan currency must match the branch treasury currency.");
        }

        var loanLimits = await GetUserLoanLimitsAsync(currentUserId);
        ValidateLoanRequestAmounts(request.Principal, request.TermMonths, loanLimits.MaxSingleLoan);

        if (loanLimits.OpenLoanCount >= loanLimits.MaxOpenLoans)
        {
            throw new InvalidOperationException(
                $"You can have at most {loanLimits.MaxOpenLoans} open loan requests or active loans at your {loanLimits.TierLabel} tier.");
        }

        if (request.Principal > loanLimits.AvailableBorrowingCapacity)
        {
            throw new InvalidOperationException(
                $"This loan would exceed your {loanLimits.TierLabel} tier borrowing limit of {loanLimits.MaxTotalOutstanding:0.##} {loanLimits.Currency}.");
        }

        var utcNow = DateTime.UtcNow;
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            BranchId = branch.Id,
            FundedAccountId = fundedAccount.Id,
            Principal = request.Principal,
            OutstandingPrincipal = request.Principal,
            InterestRatePercent = _branchOptions.DefaultInterestRatePercent,
            MonthlyPayment = CalculateMonthlyPayment(
                request.Principal,
                _branchOptions.DefaultInterestRatePercent,
                request.TermMonths),
            Currency = fundedAccount.Currency,
            TermMonths = request.TermMonths,
            Status = LoanStatus.Pending,
            Purpose = string.IsNullOrWhiteSpace(request.Purpose) ? null : request.Purpose.Trim(),
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.Loans.Add(loan);
        await _dbContext.SaveChangesAsync();

        return loan.ToResponse();
    }

    public async Task<LoanResponse?> CancelLoanRequestAsync(Guid loanId, Guid currentUserId)
    {
        var loan = await FindOwnedLoanAsync(loanId, currentUserId);

        if (loan is null)
        {
            return null;
        }

        if (loan.Status != LoanStatus.Pending)
        {
            throw new InvalidOperationException("Only pending loan requests can be cancelled.");
        }

        loan.Status = LoanStatus.Cancelled;
        loan.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return loan.ToResponse();
    }

    public async Task<LoanResponse> RepayLoanAsync(Guid loanId, RepayLoanRequest request, Guid currentUserId)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Repayment amount must be greater than zero.");
        }

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var loan = await _dbContext.Loans
                .FirstOrDefaultAsync(item => item.Id == loanId && item.UserId == currentUserId);

            if (loan is null)
            {
                throw new InvalidOperationException("Loan not found.");
            }

            if (loan.Status is not LoanStatus.Active and not LoanStatus.Overdue)
            {
                throw new InvalidOperationException("Only active loans can be repaid.");
            }

            if (request.Amount > loan.OutstandingPrincipal)
            {
                throw new InvalidOperationException("Repayment amount exceeds outstanding principal.");
            }

            var branch = await _disbursementService.GetDefaultBranchAsync();
            var treasuryAccount = await _dbContext.Accounts
                .FirstAsync(account => account.Id == branch.TreasuryAccountId);

            var sourceAccount = await _dbContext.Accounts
                .FirstOrDefaultAsync(account =>
                    account.Id == request.SourceAccountId && account.UserId == currentUserId);

            if (sourceAccount is null)
            {
                throw new InvalidOperationException("Source account not found.");
            }

            if (sourceAccount.AccountType == AccountType.Branch)
            {
                throw new InvalidOperationException("Cannot repay a loan from a branch account.");
            }

            var isGoalAccount = await _dbContext.Goals
                .AsNoTracking()
                .AnyAsync(goal => goal.GoalAccountId == sourceAccount.Id);

            if (isGoalAccount)
            {
                throw new InvalidOperationException("Cannot repay a loan from a goal account.");
            }

            if (!string.Equals(sourceAccount.Currency, loan.Currency, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Repayment currency must match the loan currency.");
            }

            if (sourceAccount.CurrentBalance < request.Amount)
            {
                throw new InvalidOperationException("Insufficient funds in the source account.");
            }

            var utcNow = DateTime.UtcNow;
            var repaymentTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                FromAccountId = sourceAccount.Id,
                ToAccountId = treasuryAccount.Id,
                Amount = request.Amount,
                Description = LoanConstants.RepaymentDescription,
                Category = TransactionCategory.Transfer,
                Reference = $"LOAN-REPAY-{utcNow:yyyyMMddHHmmss}",
                Status = TransactionStatus.Completed,
                CreatedAt = utcNow
            };

            var debitEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                TransactionId = repaymentTransaction.Id,
                AccountId = sourceAccount.Id,
                EntryType = LedgerEntryType.Debit,
                Amount = request.Amount,
                CreatedAt = utcNow
            };

            var creditEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                TransactionId = repaymentTransaction.Id,
                AccountId = treasuryAccount.Id,
                EntryType = LedgerEntryType.Credit,
                Amount = request.Amount,
                CreatedAt = utcNow
            };

            sourceAccount.CurrentBalance -= request.Amount;
            sourceAccount.UpdatedAt = utcNow;

            treasuryAccount.CurrentBalance += request.Amount;
            treasuryAccount.UpdatedAt = utcNow;

            repaymentTransaction.LedgerEntries.Add(debitEntry);
            repaymentTransaction.LedgerEntries.Add(creditEntry);
            _dbContext.Transactions.Add(repaymentTransaction);

            loan.OutstandingPrincipal -= request.Amount;
            loan.UpdatedAt = utcNow;

            if (loan.OutstandingPrincipal == 0)
            {
                loan.Status = LoanStatus.Paid;
                loan.NextDueDate = null;
            }
            else if (loan.NextDueDate.HasValue)
            {
                loan.NextDueDate = loan.NextDueDate.Value.AddMonths(1);
            }

            var repayment = new LoanRepayment
            {
                Id = Guid.NewGuid(),
                LoanId = loan.Id,
                TransactionId = repaymentTransaction.Id,
                Amount = request.Amount,
                PrincipalPortion = request.Amount,
                InterestPortion = 0,
                CreatedAt = utcNow
            };

            _dbContext.LoanRepayments.Add(repayment);
            await _dbContext.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return loan.ToResponse();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IReadOnlyList<LoanRepaymentResponse>> GetRepaymentHistoryAsync(
        Guid loanId,
        Guid currentUserId)
    {
        var loanExists = await _dbContext.Loans
            .AsNoTracking()
            .AnyAsync(loan => loan.Id == loanId && loan.UserId == currentUserId);

        if (!loanExists)
        {
            return Array.Empty<LoanRepaymentResponse>();
        }

        var repayments = await _dbContext.LoanRepayments
            .AsNoTracking()
            .Where(repayment => repayment.LoanId == loanId)
            .OrderByDescending(repayment => repayment.CreatedAt)
            .ToListAsync();

        return repayments.Select(repayment => repayment.ToResponse()).ToList();
    }

    public async Task<IReadOnlyList<LoanAdminResponse>> GetLoansForAdminAsync(LoanStatus? status)
    {
        var loansQuery = _dbContext.Loans
            .AsNoTracking()
            .Include(loan => loan.User)
            .AsQueryable();

        if (status.HasValue)
        {
            loansQuery = loansQuery.Where(loan => loan.Status == status.Value);
        }

        var loans = await loansQuery
            .OrderByDescending(loan => loan.CreatedAt)
            .ToListAsync();

        return loans.Select(loan => loan.ToAdminResponse()).ToList();
    }

    public async Task<LoanAdminResponse> ApproveLoanAsync(Guid loanId)
    {
        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var loan = await _dbContext.Loans
                .Include(item => item.FundedAccount)
                .Include(item => item.User)
                .FirstOrDefaultAsync(item => item.Id == loanId);

            if (loan is null)
            {
                throw new InvalidOperationException("Loan not found.");
            }

            if (loan.Status != LoanStatus.Pending)
            {
                throw new InvalidOperationException("Only pending loans can be approved.");
            }

            var branch = await _disbursementService.GetDefaultBranchAsync();
            var treasuryAccount = await _dbContext.Accounts
                .FirstAsync(account => account.Id == branch.TreasuryAccountId);

            await _disbursementService.ApplyDisbursementAsync(
                treasuryAccount,
                loan.FundedAccount,
                loan.Principal,
                LoanConstants.DisbursementDescription,
                TransactionCategory.Income);

            var disbursementTransaction = await _dbContext.Transactions
                .Where(transaction =>
                    transaction.FromAccountId == treasuryAccount.Id &&
                    transaction.ToAccountId == loan.FundedAccountId)
                .OrderByDescending(transaction => transaction.CreatedAt)
                .FirstAsync();

            var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
            loan.DisbursementTransactionId = disbursementTransaction.Id;
            loan.OutstandingPrincipal = loan.Principal;
            loan.MonthlyPayment = CalculateMonthlyPayment(
                loan.Principal,
                loan.InterestRatePercent,
                loan.TermMonths);
            loan.StartDate = startDate;
            loan.NextDueDate = startDate.AddMonths(1);
            loan.MaturityDate = startDate.AddMonths(loan.TermMonths);
            loan.Status = LoanStatus.Active;
            loan.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return loan.ToAdminResponse();
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<LoanAdminResponse> RejectLoanAsync(Guid loanId, RejectLoanRequest request)
    {
        var loan = await _dbContext.Loans
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.Id == loanId);

        if (loan is null)
        {
            throw new InvalidOperationException("Loan not found.");
        }

        if (loan.Status != LoanStatus.Pending)
        {
            throw new InvalidOperationException("Only pending loans can be rejected.");
        }

        var rejectionReason = request.Reason.Trim();
        if (string.IsNullOrWhiteSpace(rejectionReason))
        {
            throw new InvalidOperationException("Rejection reason is required.");
        }

        loan.Status = LoanStatus.Rejected;
        loan.RejectionReason = rejectionReason;
        loan.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return loan.ToAdminResponse();
    }

    private async Task<Loan?> FindOwnedLoanAsync(
        Guid loanId,
        Guid currentUserId,
        bool asNoTracking = false)
    {
        var query = _dbContext.Loans.AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(loan => loan.Id == loanId && loan.UserId == currentUserId);
    }

    private async Task<Account> ResolveOwnedFundedAccountAsync(Guid fundedAccountId, Guid currentUserId)
    {
        var fundedAccount = await _dbContext.Accounts
            .FirstOrDefaultAsync(account =>
                account.Id == fundedAccountId && account.UserId == currentUserId);

        if (fundedAccount is null)
        {
            throw new InvalidOperationException("Funded account not found.");
        }

        if (fundedAccount.AccountType == AccountType.Branch)
        {
            throw new InvalidOperationException("Cannot fund a loan into a branch account.");
        }

        var isGoalAccount = await _dbContext.Goals
            .AsNoTracking()
            .AnyAsync(goal => goal.GoalAccountId == fundedAccount.Id);

        if (isGoalAccount)
        {
            throw new InvalidOperationException("Cannot fund a loan into a goal account.");
        }

        return fundedAccount;
    }

    private async Task<Account> ResolveDefaultFundedAccountAsync(Guid currentUserId)
    {
        var goalAccountIds = await _dbContext.Goals
            .AsNoTracking()
            .Where(goal => goal.UserId == currentUserId)
            .Select(goal => goal.GoalAccountId)
            .ToListAsync();

        var fundedAccount = await _dbContext.Accounts
            .Where(account =>
                account.UserId == currentUserId &&
                account.AccountType != AccountType.Branch &&
                !goalAccountIds.Contains(account.Id))
            .OrderBy(account => account.AccountType == AccountType.Everyday ? 0 : 1)
            .ThenBy(account => account.CreatedAt)
            .FirstOrDefaultAsync();

        if (fundedAccount is null)
        {
            throw new InvalidOperationException("Create an account before requesting a loan.");
        }

        return fundedAccount;
    }

    private void ValidateLoanRequestAmounts(decimal principal, int termMonths, decimal maxSingleLoan)
    {
        if (principal < _branchOptions.MinLoanAmount)
        {
            throw new InvalidOperationException(
                $"Loan amount must be at least {_branchOptions.MinLoanAmount:0.##}.");
        }

        var effectiveMaxSingleLoan = Math.Min(maxSingleLoan, _branchOptions.MaxLoanAmount);
        if (principal > effectiveMaxSingleLoan)
        {
            throw new InvalidOperationException(
                $"Loan amount cannot exceed {effectiveMaxSingleLoan:0.##} for your current tier.");
        }

        if (termMonths <= 0)
        {
            throw new InvalidOperationException("Loan term must be greater than zero.");
        }

        if (termMonths > _branchOptions.MaxTermMonths)
        {
            throw new InvalidOperationException(
                $"Loan term cannot exceed {_branchOptions.MaxTermMonths} months.");
        }
    }

    private async Task<decimal> GetUserHoldingsAsync(Guid currentUserId, string currency)
    {
        var goalAccountIds = await _dbContext.Goals
            .AsNoTracking()
            .Where(goal => goal.UserId == currentUserId)
            .Select(goal => goal.GoalAccountId)
            .ToListAsync();

        return await _dbContext.Accounts
            .AsNoTracking()
            .Where(account =>
                account.UserId == currentUserId &&
                account.AccountType != AccountType.Branch &&
                !goalAccountIds.Contains(account.Id) &&
                account.Currency == currency)
            .SumAsync(account => account.CurrentBalance);
    }

    private LoanLimitTierOptions ResolveLoanLimitTier(decimal totalHoldings)
    {
        var configuredTiers = _branchOptions.LoanLimitTiers
            .Where(tier => tier.MaxOpenLoans > 0 && tier.MaxSingleLoan > 0 && tier.MaxTotalOutstanding > 0)
            .OrderByDescending(tier => tier.MinHoldings)
            .ToList();

        if (configuredTiers.Count == 0)
        {
            return new LoanLimitTierOptions
            {
                MinHoldings = 0,
                Label = "Standard",
                MaxSingleLoan = _branchOptions.MaxLoanAmount,
                MaxTotalOutstanding = _branchOptions.MaxLoanAmount * _branchOptions.MaxActiveLoansPerUser,
                MaxOpenLoans = _branchOptions.MaxActiveLoansPerUser
            };
        }

        var matchedTier = configuredTiers.FirstOrDefault(tier => totalHoldings >= tier.MinHoldings)
            ?? configuredTiers[^1];

        return matchedTier;
    }

    private async Task<(int OpenLoanCount, decimal TotalExposure)> GetOpenLoanExposureAsync(Guid currentUserId)
    {
        var openLoans = await _dbContext.Loans
            .AsNoTracking()
            .Where(loan =>
                loan.UserId == currentUserId &&
                (loan.Status == LoanStatus.Pending ||
                 loan.Status == LoanStatus.Active ||
                 loan.Status == LoanStatus.Overdue))
            .ToListAsync();

        var totalExposure = openLoans.Sum(loan =>
            loan.Status == LoanStatus.Pending ? loan.Principal : loan.OutstandingPrincipal);

        return (openLoans.Count, totalExposure);
    }

    internal static decimal CalculateMonthlyPayment(
        decimal principal,
        decimal annualInterestRatePercent,
        int termMonths)
    {
        if (termMonths <= 0)
        {
            throw new InvalidOperationException("Loan term must be greater than zero.");
        }

        if (annualInterestRatePercent <= 0)
        {
            return Math.Round(principal / termMonths, 2);
        }

        var monthlyRate = (double)(annualInterestRatePercent / 100m / 12m);
        var payment = (double)principal
            * monthlyRate
            * Math.Pow(1 + monthlyRate, termMonths)
            / (Math.Pow(1 + monthlyRate, termMonths) - 1);

        return Math.Round((decimal)payment, 2);
    }
}
