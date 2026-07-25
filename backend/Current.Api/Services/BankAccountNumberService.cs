using Current.Api.Common.Constants;
using Current.Api.Data;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class BankAccountNumberService : IBankAccountNumberService
{
    private readonly ApplicationDbContext _dbContext;

    public BankAccountNumberService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AssignBankDetailsAsync(Account account)
    {
        account.Bsb = BankAccountConstants.DefaultBsb;
        account.AccountNumber = await GenerateNextAccountNumberAsync(BankAccountConstants.DefaultBsb);
    }

    public Task AssignBranchTreasuryDetailsAsync(Account treasuryAccount)
    {
        treasuryAccount.Bsb = BankAccountConstants.BranchTreasuryBsb;
        treasuryAccount.AccountNumber = BankAccountConstants.BranchTreasuryAccountNumber;
        return Task.CompletedTask;
    }

    private async Task<string> GenerateNextAccountNumberAsync(string bsb)
    {
        var existingAccountNumbers = await _dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.Bsb == bsb)
            .Select(account => account.AccountNumber)
            .ToListAsync();

        var highestAccountNumber = existingAccountNumbers
            .Select(ParseAccountNumber)
            .DefaultIfEmpty(BankAccountConstants.AccountNumberStart - 1)
            .Max();

        var nextAccountNumber = highestAccountNumber + 1;

        return nextAccountNumber.ToString().PadLeft(BankAccountConstants.AccountNumberLength, '0');
    }

    private static int ParseAccountNumber(string accountNumber)
    {
        return int.TryParse(accountNumber, out var parsedAccountNumber)
            ? parsedAccountNumber
            : BankAccountConstants.AccountNumberStart - 1;
    }
}
