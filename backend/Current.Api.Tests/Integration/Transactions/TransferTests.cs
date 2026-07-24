using System.Net;
using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.DTOs.Transactions;
using Current.Api.Entities;
using Current.Api.Tests.Helpers;
using Current.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Current.Api.Tests.Integration.Transactions;

public class TransferTests : IntegrationTestBase
{
    private const string DefaultPassword = "Password123";

    public TransferTests(CurrentApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Transfer_ValidRequest_UpdatesBalancesAndCreatesLedgerEntries()
    {
        var transferContext = await SeedUserWithTwoAccountsAsync(
            "transfer-happy@example.com",
            fromBalance: 1000m,
            toBalance: 250m);

        var authenticatedClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            transferContext.User.Email,
            DefaultPassword);

        var transferRequest = new TransferRequest
        {
            FromAccountId = transferContext.FromAccount.Id,
            ToAccountId = transferContext.ToAccount.Id,
            Amount = 300m,
            Description = "Move to savings",
        };

        var response = await authenticatedClient.PostJsonAsync("/transactions/transfer", transferRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var transaction = await response.ReadJsonAsync<TransactionResponse>();

        Assert.NotNull(transaction);
        Assert.Equal(300m, transaction.Amount);
        Assert.Equal(2, transaction.LedgerEntries.Count);

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await LedgerAssertions.AssertAccountBalanceAsync(dbContext, transferContext.FromAccount.Id, 700m);
        await LedgerAssertions.AssertAccountBalanceAsync(dbContext, transferContext.ToAccount.Id, 550m);

        var ledgerEntryCount = await dbContext.LedgerEntries.CountAsync();
        Assert.Equal(2, ledgerEntryCount);
    }

    [Fact]
    public async Task Transfer_ValidRequest_DebitAmountEqualsCreditAmount()
    {
        var transferContext = await SeedUserWithTwoAccountsAsync(
            "transfer-ledger@example.com",
            fromBalance: 500m,
            toBalance: 100m);

        var authenticatedClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            transferContext.User.Email,
            DefaultPassword);

        var transferRequest = new TransferRequest
        {
            FromAccountId = transferContext.FromAccount.Id,
            ToAccountId = transferContext.ToAccount.Id,
            Amount = 125.50m,
            Description = "Ledger balance check",
        };

        var response = await authenticatedClient.PostJsonAsync("/transactions/transfer", transferRequest);
        var transaction = await response.ReadJsonAsync<TransactionResponse>();

        Assert.NotNull(transaction);

        var debitEntry = transaction.LedgerEntries.Single(entry => entry.EntryType == LedgerEntryType.Debit);
        var creditEntry = transaction.LedgerEntries.Single(entry => entry.EntryType == LedgerEntryType.Credit);

        Assert.Equal(125.50m, debitEntry.Amount);
        Assert.Equal(debitEntry.Amount, creditEntry.Amount);
        Assert.Equal(transferContext.FromAccount.Id, debitEntry.AccountId);
        Assert.Equal(transferContext.ToAccount.Id, creditEntry.AccountId);
    }

    [Fact]
    public async Task Transfer_InsufficientFunds_ReturnsBadRequestAndKeepsBalances()
    {
        var transferContext = await SeedUserWithTwoAccountsAsync(
            "transfer-insufficient@example.com",
            fromBalance: 100m,
            toBalance: 50m);

        var authenticatedClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            transferContext.User.Email,
            DefaultPassword);

        var transferRequest = new TransferRequest
        {
            FromAccountId = transferContext.FromAccount.Id,
            ToAccountId = transferContext.ToAccount.Id,
            Amount = 500m,
            Description = "Too much",
        };

        var response = await authenticatedClient.PostJsonAsync("/transactions/transfer", transferRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await LedgerAssertions.AssertAccountBalanceAsync(dbContext, transferContext.FromAccount.Id, 100m);
        await LedgerAssertions.AssertAccountBalanceAsync(dbContext, transferContext.ToAccount.Id, 50m);

        var transactionCount = await dbContext.Transactions.CountAsync();
        Assert.Equal(0, transactionCount);
    }

    [Fact]
    public async Task Transfer_SameAccount_ReturnsBadRequest()
    {
        var transferContext = await SeedUserWithTwoAccountsAsync(
            "transfer-same-account@example.com",
            fromBalance: 500m,
            toBalance: 100m);

        var authenticatedClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            transferContext.User.Email,
            DefaultPassword);

        var transferRequest = new TransferRequest
        {
            FromAccountId = transferContext.FromAccount.Id,
            ToAccountId = transferContext.FromAccount.Id,
            Amount = 50m,
            Description = "Same account",
        };

        var response = await authenticatedClient.PostJsonAsync("/transactions/transfer", transferRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_NonPositiveAmount_ReturnsBadRequest()
    {
        var transferContext = await SeedUserWithTwoAccountsAsync(
            "transfer-zero@example.com",
            fromBalance: 500m,
            toBalance: 100m);

        var authenticatedClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            transferContext.User.Email,
            DefaultPassword);

        var transferRequest = new TransferRequest
        {
            FromAccountId = transferContext.FromAccount.Id,
            ToAccountId = transferContext.ToAccount.Id,
            Amount = 0m,
            Description = "Zero amount",
        };

        var response = await authenticatedClient.PostJsonAsync("/transactions/transfer", transferRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_OtherUsersDestinationAccount_ReturnsBadRequest()
    {
        await SeedUserWithTwoAccountsAsync("transfer-owner@example.com", 1000m, 200m);
        var otherUser = await SeedUserAsync("transfer-other@example.com");

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var owner = await dbContext.Users.SingleAsync(user => user.Email == "transfer-owner@example.com");
        var ownerFromAccount = await dbContext.Accounts.SingleAsync(account =>
            account.UserId == owner.Id && account.Name == "Everyday");

        var otherUserAccount = await TestDataSeeder.SeedAccountAsync(
            dbContext,
            otherUser.Id,
            "Other Bills",
            AccountType.Everyday,
            300m);

        var ownerClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            "transfer-owner@example.com",
            DefaultPassword);

        var transferRequest = new TransferRequest
        {
            FromAccountId = ownerFromAccount.Id,
            ToAccountId = otherUserAccount.Id,
            Amount = 100m,
            Description = "Cross-user transfer",
        };

        var response = await ownerClient.PostJsonAsync("/transactions/transfer", transferRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Transfer_ValidRequest_CreatesTransferCompletedNotification()
    {
        var transferContext = await SeedUserWithTwoAccountsAsync(
            "transfer-notify@example.com",
            fromBalance: 800m,
            toBalance: 100m);

        var authenticatedClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            transferContext.User.Email,
            DefaultPassword);

        var transferRequest = new TransferRequest
        {
            FromAccountId = transferContext.FromAccount.Id,
            ToAccountId = transferContext.ToAccount.Id,
            Amount = 200m,
            Description = "Notify me",
        };

        await authenticatedClient.PostJsonAsync("/transactions/transfer", transferRequest);

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var transferNotificationExists = await dbContext.Notifications
            .AsNoTracking()
            .AnyAsync(notification =>
                notification.UserId == transferContext.User.Id &&
                notification.NotificationType == NotificationType.System &&
                notification.Title == "Transfer completed");

        Assert.True(transferNotificationExists);
    }

    private async Task<User> SeedUserAsync(string email)
    {
        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        return await TestDataSeeder.SeedUserAsync(
            dbContext,
            passwordHasher,
            "Test",
            "User",
            email,
            DefaultPassword);
    }

    private async Task<TransferTestContext> SeedUserWithTwoAccountsAsync(
        string email,
        decimal fromBalance,
        decimal toBalance)
    {
        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        var user = await TestDataSeeder.SeedUserAsync(
            dbContext,
            passwordHasher,
            "Test",
            "User",
            email,
            DefaultPassword);

        var fromAccount = await TestDataSeeder.SeedAccountAsync(
            dbContext,
            user.Id,
            "Everyday",
            AccountType.Everyday,
            fromBalance);

        var toAccount = await TestDataSeeder.SeedAccountAsync(
            dbContext,
            user.Id,
            "Savings",
            AccountType.Savings,
            toBalance);

        return new TransferTestContext(user, fromAccount, toAccount);
    }

    private sealed record TransferTestContext(User User, Account FromAccount, Account ToAccount);
}
