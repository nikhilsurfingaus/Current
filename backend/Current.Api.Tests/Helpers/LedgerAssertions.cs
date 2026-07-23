using Current.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Tests.Helpers;

public static class LedgerAssertions
{
    public static async Task<decimal> GetAccountBalanceAsync(
        ApplicationDbContext dbContext,
        Guid accountId)
    {
        var accountBalance = await dbContext.Accounts
            .AsNoTracking()
            .Where(account => account.Id == accountId)
            .Select(account => account.CurrentBalance)
            .SingleAsync();

        return accountBalance;
    }

    public static async Task AssertAccountBalanceAsync(
        ApplicationDbContext dbContext,
        Guid accountId,
        decimal expectedBalance)
    {
        var actualBalance = await GetAccountBalanceAsync(dbContext, accountId);

        Assert.Equal(expectedBalance, actualBalance);
    }
}
