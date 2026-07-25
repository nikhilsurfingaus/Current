using Current.Api.Common;
using Current.Api.Common.Constants;
using Current.Api.Common.Enums;
using Current.Api.Configuration;
using Current.Api.Data;
using Current.Api.DTOs.Accounts;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Current.Api.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Current.Api.Services;

public class AccountService : IAccountService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDisbursementService _disbursementService;
    private readonly INotificationService _notificationService;
    private readonly IBankAccountNumberService _bankAccountNumberService;
    private readonly BranchOptions _branchOptions;

    public AccountService(
        ApplicationDbContext dbContext,
        IDisbursementService disbursementService,
        INotificationService notificationService,
        IBankAccountNumberService bankAccountNumberService,
        IOptions<BranchOptions> branchOptions)
    {
        _dbContext = dbContext;
        _disbursementService = disbursementService;
        _notificationService = notificationService;
        _bankAccountNumberService = bankAccountNumberService;
        _branchOptions = branchOptions.Value;
    }

    public async Task<IReadOnlyList<AccountResponse>> GetAllAccountsAsync(Guid currentUserId)
    {
        var accounts = await _dbContext.Accounts
            .AsNoTracking()
            .Where(account =>
                account.UserId == currentUserId &&
                account.AccountType != AccountType.Branch)
            .OrderBy(account => account.Name)
            .ToListAsync();

        return accounts.Select(account => account.ToResponse()).ToList();
    }

    public async Task<AccountResponse?> GetAccountByIdAsync(Guid accountId, Guid currentUserId)
    {
        var account = await _dbContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(account =>
                account.Id == accountId &&
                account.UserId == currentUserId &&
                account.AccountType != AccountType.Branch);

        return account?.ToResponse();
    }

    public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, Guid currentUserId)
    {
        var normalizedCurrency = request.Currency.Trim().ToUpperInvariant();

        if (normalizedCurrency.Length != 3)
        {
            throw new InvalidOperationException("Currency must be a 3-letter code.");
        }

        if (request.AccountType == AccountType.Branch)
        {
            throw new InvalidOperationException("Branch accounts cannot be created by users.");
        }

        var goalAccountIds = await _dbContext.Goals
            .AsNoTracking()
            .Where(goal => goal.UserId == currentUserId)
            .Select(goal => goal.GoalAccountId)
            .ToListAsync();

        var existingUserAccountCount = await _dbContext.Accounts
            .AsNoTracking()
            .CountAsync(account =>
                account.UserId == currentUserId &&
                account.AccountType != AccountType.Branch &&
                !goalAccountIds.Contains(account.Id));

        var welcomeCreditEligible = existingUserAccountCount < _branchOptions.WelcomeCreditMaxAccounts
            && _branchOptions.WelcomeCreditAmount > 0;

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var utcNow = DateTime.UtcNow;

            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Name = request.Name.Trim(),
                AccountType = request.AccountType,
                CurrentBalance = 0,
                Currency = normalizedCurrency,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            };

            await _bankAccountNumberService.AssignBankDetailsAsync(account);

            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync();

            decimal? welcomeCreditAmount = null;

            if (welcomeCreditEligible)
            {
                var branch = await _disbursementService.GetDefaultBranchAsync();
                var treasuryAccount = await _dbContext.Accounts
                    .FirstAsync(treasury => treasury.Id == branch.TreasuryAccountId);

                if (!string.Equals(treasuryAccount.Currency, account.Currency, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "Welcome credit is not available for this currency yet.");
                }

                await _disbursementService.ApplyDisbursementAsync(
                    treasuryAccount,
                    account,
                    _branchOptions.WelcomeCreditAmount,
                    BranchConstants.WelcomeCreditDescription,
                    TransactionCategory.Income);

                await _dbContext.SaveChangesAsync();
                welcomeCreditAmount = _branchOptions.WelcomeCreditAmount;
            }

            await dbTransaction.CommitAsync();

            await _notificationService.TryCreateNotificationAsync(
                currentUserId,
                NotificationType.AccountCreated,
                "Account created",
                $"{account.Name} is ready to use.");

            if (welcomeCreditAmount.HasValue)
            {
                await _notificationService.TryCreateNotificationAsync(
                    currentUserId,
                    NotificationType.System,
                    "Welcome credit",
                    $"You received {NotificationFormatting.FormatAmount(welcomeCreditAmount.Value, account.Currency)} from Current HQ.");
            }

            var response = account.ToResponse();
            response.WelcomeCreditAmount = welcomeCreditAmount;
            return response;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }
}
