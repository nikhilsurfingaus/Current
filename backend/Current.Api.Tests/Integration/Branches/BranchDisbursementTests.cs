using System.Net;
using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.DTOs.Branches;
using Current.Api.Entities;
using Current.Api.Tests.Helpers;
using Current.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Current.Api.Tests.Integration.Branches;

public class BranchDisbursementTests : IntegrationTestBase
{
    private const string DefaultPassword = "Password123";

    public BranchDisbursementTests(CurrentApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateDisbursement_ByEmail_UpdatesRecipientBalance()
    {
        var disbursementContext = await SeedDisbursementContextAsync();
        var adminClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            disbursementContext.Admin.Email,
            DefaultPassword);

        var response = await adminClient.PostJsonAsync("/branch/disbursements", new CreateBranchDisbursementRequest
        {
            RecipientEmail = disbursementContext.Recipient.Email,
            Amount = 500m,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var disbursement = await response.ReadJsonAsync<BranchDisbursementResponse>();
        Assert.NotNull(disbursement);
        Assert.Equal(500m, disbursement.Amount);
        Assert.Equal(disbursementContext.RecipientAccount.Id, disbursement.RecipientAccountId);

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await LedgerAssertions.AssertAccountBalanceAsync(
            dbContext,
            disbursementContext.RecipientAccount.Id,
            2500m);
    }

    [Fact]
    public async Task CreateDisbursement_ByBsbAndAccountNumber_UpdatesRecipientBalance()
    {
        var disbursementContext = await SeedDisbursementContextAsync();
        var adminClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            disbursementContext.Admin.Email,
            DefaultPassword);

        var response = await adminClient.PostJsonAsync("/branch/disbursements", new CreateBranchDisbursementRequest
        {
            RecipientBsb = disbursementContext.RecipientAccount.Bsb,
            RecipientAccountNumber = disbursementContext.RecipientAccount.AccountNumber,
            Amount = 750m,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var disbursement = await response.ReadJsonAsync<BranchDisbursementResponse>();
        Assert.NotNull(disbursement);
        Assert.Equal(750m, disbursement.Amount);
        Assert.Equal(disbursementContext.RecipientAccount.Id, disbursement.RecipientAccountId);

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await LedgerAssertions.AssertAccountBalanceAsync(
            dbContext,
            disbursementContext.RecipientAccount.Id,
            2750m);
    }

    private async Task<DisbursementTestContext> SeedDisbursementContextAsync()
    {
        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        await TestDataSeeder.SeedBranchTreasuryAsync(dbContext);

        var recipient = await TestDataSeeder.SeedUserAsync(
            dbContext,
            passwordHasher,
            "TopUp",
            "Recipient",
            $"recipient-{Guid.NewGuid():N}@example.com",
            DefaultPassword);

        var admin = await TestDataSeeder.SeedUserAsync(
            dbContext,
            passwordHasher,
            "Branch",
            "Admin",
            $"admin-{Guid.NewGuid():N}@example.com",
            DefaultPassword,
            UserRole.Admin);

        var recipientAccount = await TestDataSeeder.SeedAccountAsync(
            dbContext,
            recipient.Id,
            "Everyday",
            AccountType.Everyday,
            2000m);

        return new DisbursementTestContext(recipient, admin, recipientAccount);
    }

    private sealed record DisbursementTestContext(
        User Recipient,
        User Admin,
        Account RecipientAccount);
}
