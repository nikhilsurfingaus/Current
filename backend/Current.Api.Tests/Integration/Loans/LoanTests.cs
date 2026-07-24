using System.Net;
using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.DTOs.Loans;
using Current.Api.Entities;
using Current.Api.Tests.Helpers;
using Current.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Current.Api.Tests.Integration.Loans;

public class LoanTests : IntegrationTestBase
{
    private const string DefaultPassword = "Password123";

    public LoanTests(CurrentApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateLoanRequest_ValidRequest_ReturnsCreatedPendingLoan()
    {
        var loanContext = await SeedLoanContextAsync();
        var borrowerClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            loanContext.Borrower.Email,
            DefaultPassword);

        var createLoanRequest = new CreateLoanRequest
        {
            Principal = 1000m,
            TermMonths = 12,
            FundedAccountId = loanContext.BorrowerAccount.Id,
            Purpose = "Car repair",
        };

        var response = await borrowerClient.PostJsonAsync("/loans", createLoanRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var loan = await response.ReadJsonAsync<LoanResponse>();

        Assert.NotNull(loan);
        Assert.Equal(LoanStatus.Pending, loan.Status);
        Assert.Equal(1000m, loan.Principal);
        Assert.Equal(loanContext.BorrowerAccount.Id, loan.FundedAccountId);
    }

    [Fact]
    public async Task ApproveLoan_PendingLoan_DisburseFundsAndActivatesLoan()
    {
        var loanContext = await SeedLoanContextAsync();
        var loanId = await CreatePendingLoanAsync(loanContext);

        var adminClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            loanContext.Admin.Email,
            DefaultPassword);

        var response = await adminClient.PostJsonAsync($"/branch/loans/{loanId}/approve", new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var approvedLoan = await response.ReadJsonAsync<LoanResponse>();

        Assert.NotNull(approvedLoan);
        Assert.Equal(LoanStatus.Active, approvedLoan.Status);

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await LedgerAssertions.AssertAccountBalanceAsync(dbContext, loanContext.BorrowerAccount.Id, 3000m);
        await LedgerAssertions.AssertAccountBalanceAsync(
            dbContext,
            loanContext.TreasuryAccount.Id,
            loanContext.InitialTreasuryBalance - 1000m);
    }

    [Fact]
    public async Task RejectLoan_PendingLoan_UpdatesStatusAndNotifiesBorrower()
    {
        var loanContext = await SeedLoanContextAsync();
        var loanId = await CreatePendingLoanAsync(loanContext);

        var adminClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            loanContext.Admin.Email,
            DefaultPassword);

        var rejectRequest = new RejectLoanRequest
        {
            Reason = "Insufficient documentation",
        };

        var response = await adminClient.PostJsonAsync($"/branch/loans/{loanId}/reject", rejectRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var rejectedLoan = await response.ReadJsonAsync<LoanResponse>();

        Assert.NotNull(rejectedLoan);
        Assert.Equal(LoanStatus.Rejected, rejectedLoan.Status);
        Assert.Equal("Insufficient documentation", rejectedLoan.RejectionReason);

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var rejectionNotificationExists = await dbContext.Notifications
            .AsNoTracking()
            .AnyAsync(notification =>
                notification.UserId == loanContext.Borrower.Id &&
                notification.NotificationType == NotificationType.System &&
                notification.Title == "Loan request rejected");

        Assert.True(rejectionNotificationExists);
    }

    [Fact]
    public async Task ApproveLoan_NonAdminUser_ReturnsForbidden()
    {
        var loanContext = await SeedLoanContextAsync();
        var loanId = await CreatePendingLoanAsync(loanContext);

        var borrowerClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            loanContext.Borrower.Email,
            DefaultPassword);

        var response = await borrowerClient.PostJsonAsync($"/branch/loans/{loanId}/approve", new { });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RepayLoan_ActiveLoan_ReducesOutstandingPrincipal()
    {
        var loanContext = await SeedLoanContextAsync();
        var loanId = await CreatePendingLoanAsync(loanContext);

        var adminClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            loanContext.Admin.Email,
            DefaultPassword);

        await adminClient.PostJsonAsync($"/branch/loans/{loanId}/approve", new { });

        var borrowerClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            loanContext.Borrower.Email,
            DefaultPassword);

        var repayRequest = new RepayLoanRequest
        {
            Amount = 250m,
            SourceAccountId = loanContext.BorrowerAccount.Id,
        };

        var response = await borrowerClient.PostJsonAsync($"/loans/{loanId}/repay", repayRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var repaidLoan = await response.ReadJsonAsync<LoanResponse>();

        Assert.NotNull(repaidLoan);
        Assert.Equal(750m, repaidLoan.OutstandingPrincipal);
        Assert.Equal(LoanStatus.Active, repaidLoan.Status);
    }

    [Fact]
    public async Task RepayLoan_AmountExceedsOutstanding_ReturnsBadRequest()
    {
        var loanContext = await SeedLoanContextAsync();
        var loanId = await CreatePendingLoanAsync(loanContext);

        var adminClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            loanContext.Admin.Email,
            DefaultPassword);

        await adminClient.PostJsonAsync($"/branch/loans/{loanId}/approve", new { });

        var borrowerClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            loanContext.Borrower.Email,
            DefaultPassword);

        var repayRequest = new RepayLoanRequest
        {
            Amount = 1500m,
            SourceAccountId = loanContext.BorrowerAccount.Id,
        };

        var response = await borrowerClient.PostJsonAsync($"/loans/{loanId}/repay", repayRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<Guid> CreatePendingLoanAsync(LoanTestContext loanContext)
    {
        var borrowerClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            loanContext.Borrower.Email,
            DefaultPassword);

        var createLoanRequest = new CreateLoanRequest
        {
            Principal = 1000m,
            TermMonths = 12,
            FundedAccountId = loanContext.BorrowerAccount.Id,
        };

        var response = await borrowerClient.PostJsonAsync("/loans", createLoanRequest);
        var loan = await response.ReadJsonAsync<LoanResponse>();

        Assert.NotNull(loan);

        return loan.Id;
    }

    private async Task<LoanTestContext> SeedLoanContextAsync()
    {
        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        var (_, treasuryAccount) = await TestDataSeeder.SeedBranchTreasuryAsync(dbContext);

        var borrower = await TestDataSeeder.SeedUserAsync(
            dbContext,
            passwordHasher,
            "Borrower",
            "User",
            $"borrower-{Guid.NewGuid():N}@example.com",
            DefaultPassword);

        var admin = await TestDataSeeder.SeedUserAsync(
            dbContext,
            passwordHasher,
            "Branch",
            "Admin",
            $"admin-{Guid.NewGuid():N}@example.com",
            DefaultPassword,
            UserRole.Admin);

        var borrowerAccount = await TestDataSeeder.SeedAccountAsync(
            dbContext,
            borrower.Id,
            "Everyday",
            AccountType.Everyday,
            2000m);

        return new LoanTestContext(
            borrower,
            admin,
            borrowerAccount,
            treasuryAccount,
            treasuryAccount.CurrentBalance);
    }

    private sealed record LoanTestContext(
        User Borrower,
        User Admin,
        Account BorrowerAccount,
        Account TreasuryAccount,
        decimal InitialTreasuryBalance);
}
