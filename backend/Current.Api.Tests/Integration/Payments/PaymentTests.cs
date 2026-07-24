using System.Net;
using Current.Api.Common.Enums;
using Current.Api.Data;
using Current.Api.DTOs.Payments;
using Current.Api.Entities;
using Current.Api.Tests.Helpers;
using Current.Api.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Current.Api.Tests.Integration.Payments;

public class PaymentTests : IntegrationTestBase
{
    private const string DefaultPassword = "Password123";

    public PaymentTests(CurrentApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task SendPayment_ValidRequest_UpdatesBalances()
    {
        var paymentParties = await SeedPaymentPartiesAsync();
        var senderClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            paymentParties.Sender.Email,
            DefaultPassword);

        var paymentRequest = CreatePaymentRequest(
            paymentParties.SenderAccount.Id,
            paymentParties.Recipient.Email,
            150m);

        var response = await senderClient.PostJsonAsync(
            "/payments/send",
            paymentRequest,
            CreateIdempotencyHeaders("payment-happy-path"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var receipt = await response.ReadJsonAsync<PaymentReceiptResponse>();

        Assert.NotNull(receipt);
        Assert.Equal(150m, receipt.Amount);

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await LedgerAssertions.AssertAccountBalanceAsync(dbContext, paymentParties.SenderAccount.Id, 850m);
        await LedgerAssertions.AssertAccountBalanceAsync(dbContext, paymentParties.RecipientAccount.Id, 250m);
    }

    [Fact]
    public async Task SendPayment_SelfPayment_ReturnsBadRequest()
    {
        var paymentParties = await SeedPaymentPartiesAsync();
        var senderClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            paymentParties.Sender.Email,
            DefaultPassword);

        var paymentRequest = CreatePaymentRequest(
            paymentParties.SenderAccount.Id,
            paymentParties.Sender.Email,
            50m);

        var response = await senderClient.PostJsonAsync(
            "/payments/send",
            paymentRequest,
            CreateIdempotencyHeaders("payment-self"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.ReadJsonAsync<PaymentErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal(PaymentErrorCode.SelfPaymentNotAllowed, error.Code);
    }

    [Fact]
    public async Task SendPayment_InsufficientFunds_ReturnsBadRequest()
    {
        var paymentParties = await SeedPaymentPartiesAsync();
        var senderClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            paymentParties.Sender.Email,
            DefaultPassword);

        var paymentRequest = CreatePaymentRequest(
            paymentParties.SenderAccount.Id,
            paymentParties.Recipient.Email,
            5000m);

        var response = await senderClient.PostJsonAsync(
            "/payments/send",
            paymentRequest,
            CreateIdempotencyHeaders("payment-insufficient"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.ReadJsonAsync<PaymentErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal(PaymentErrorCode.InsufficientFunds, error.Code);

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await LedgerAssertions.AssertAccountBalanceAsync(dbContext, paymentParties.SenderAccount.Id, 1000m);
    }

    [Fact]
    public async Task SendPayment_RecipientNotFound_ReturnsBadRequest()
    {
        var paymentParties = await SeedPaymentPartiesAsync();
        var senderClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            paymentParties.Sender.Email,
            DefaultPassword);

        var paymentRequest = CreatePaymentRequest(
            paymentParties.SenderAccount.Id,
            "missing-user@example.com",
            25m);

        var response = await senderClient.PostJsonAsync(
            "/payments/send",
            paymentRequest,
            CreateIdempotencyHeaders("payment-missing-recipient"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.ReadJsonAsync<PaymentErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal(PaymentErrorCode.RecipientNotFound, error.Code);
    }

    [Fact]
    public async Task SendPayment_SameIdempotencyKey_ReplaysReceiptWithoutDoubleDebit()
    {
        var paymentParties = await SeedPaymentPartiesAsync();
        var senderClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            paymentParties.Sender.Email,
            DefaultPassword);

        var paymentRequest = CreatePaymentRequest(
            paymentParties.SenderAccount.Id,
            paymentParties.Recipient.Email,
            75m);

        const string idempotencyKey = "payment-replay-key";
        var headers = CreateIdempotencyHeaders(idempotencyKey);

        var firstResponse = await senderClient.PostJsonAsync("/payments/send", paymentRequest, headers);
        var secondResponse = await senderClient.PostJsonAsync("/payments/send", paymentRequest, headers);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        var firstReceipt = await firstResponse.ReadJsonAsync<PaymentReceiptResponse>();
        var secondReceipt = await secondResponse.ReadJsonAsync<PaymentReceiptResponse>();

        Assert.NotNull(firstReceipt);
        Assert.NotNull(secondReceipt);
        Assert.Equal(firstReceipt.TransactionId, secondReceipt.TransactionId);

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await LedgerAssertions.AssertAccountBalanceAsync(dbContext, paymentParties.SenderAccount.Id, 925m);
        await LedgerAssertions.AssertAccountBalanceAsync(dbContext, paymentParties.RecipientAccount.Id, 175m);

        var transactionCount = await dbContext.Transactions.CountAsync();
        Assert.Equal(1, transactionCount);
    }

    [Fact]
    public async Task SendPayment_SameIdempotencyKeyDifferentAmount_ReturnsDuplicatePaymentError()
    {
        var paymentParties = await SeedPaymentPartiesAsync();
        var senderClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            paymentParties.Sender.Email,
            DefaultPassword);

        const string idempotencyKey = "payment-duplicate-key";
        var headers = CreateIdempotencyHeaders(idempotencyKey);

        var firstRequest = CreatePaymentRequest(
            paymentParties.SenderAccount.Id,
            paymentParties.Recipient.Email,
            40m);

        var secondRequest = CreatePaymentRequest(
            paymentParties.SenderAccount.Id,
            paymentParties.Recipient.Email,
            41m);

        var firstResponse = await senderClient.PostJsonAsync("/payments/send", firstRequest, headers);
        var secondResponse = await senderClient.PostJsonAsync("/payments/send", secondRequest, headers);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);

        var error = await secondResponse.ReadJsonAsync<PaymentErrorResponse>();

        Assert.NotNull(error);
        Assert.Equal(PaymentErrorCode.DuplicatePayment, error.Code);
    }

    [Fact]
    public async Task SendPayment_ValidRequest_CreatesPaymentReceivedNotificationForRecipientOnly()
    {
        var paymentParties = await SeedPaymentPartiesAsync();
        var senderClient = await Factory.CreateAuthenticatedClientViaLoginAsync(
            paymentParties.Sender.Email,
            DefaultPassword);

        var paymentRequest = CreatePaymentRequest(
            paymentParties.SenderAccount.Id,
            paymentParties.Recipient.Email,
            60m);

        var response = await senderClient.PostJsonAsync(
            "/payments/send",
            paymentRequest,
            CreateIdempotencyHeaders("payment-notification"));

        var receipt = await response.ReadJsonAsync<PaymentReceiptResponse>();

        Assert.NotNull(receipt);

        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var recipientNotificationCount = await dbContext.Notifications
            .CountAsync(notification =>
                notification.UserId == paymentParties.Recipient.Id &&
                notification.NotificationType == NotificationType.PaymentReceived &&
                notification.RelatedEntityId == receipt.TransactionId);

        var senderNotificationCount = await dbContext.Notifications
            .CountAsync(notification =>
                notification.UserId == paymentParties.Sender.Id &&
                notification.NotificationType == NotificationType.PaymentSent);

        Assert.Equal(1, recipientNotificationCount);
        Assert.Equal(0, senderNotificationCount);
    }

    private static SendPaymentRequest CreatePaymentRequest(
        Guid fromAccountId,
        string recipientEmail,
        decimal amount)
    {
        return new SendPaymentRequest
        {
            FromAccountId = fromAccountId,
            RecipientEmail = recipientEmail,
            Amount = amount,
            Reference = "Test payment",
        };
    }

    private static Dictionary<string, string> CreateIdempotencyHeaders(string idempotencyKey)
    {
        return new Dictionary<string, string>
        {
            ["Idempotency-Key"] = idempotencyKey,
        };
    }

    private async Task<PaymentPartiesContext> SeedPaymentPartiesAsync()
    {
        using var scope = Factory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        var sender = await TestDataSeeder.SeedUserAsync(
            dbContext,
            passwordHasher,
            "Nikhil",
            "Naik",
            $"sender-{Guid.NewGuid():N}@example.com",
            DefaultPassword);

        var recipient = await TestDataSeeder.SeedUserAsync(
            dbContext,
            passwordHasher,
            "Mirabel",
            "Suttcliffe",
            $"recipient-{Guid.NewGuid():N}@example.com",
            DefaultPassword);

        var senderAccount = await TestDataSeeder.SeedAccountAsync(
            dbContext,
            sender.Id,
            "Everyday",
            AccountType.Everyday,
            1000m);

        var recipientAccount = await TestDataSeeder.SeedAccountAsync(
            dbContext,
            recipient.Id,
            "Bills",
            AccountType.Everyday,
            100m);

        return new PaymentPartiesContext(sender, recipient, senderAccount, recipientAccount);
    }

    private sealed record PaymentPartiesContext(
        User Sender,
        User Recipient,
        Account SenderAccount,
        Account RecipientAccount);
}
