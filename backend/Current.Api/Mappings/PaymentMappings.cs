using Current.Api.DTOs.Payments;
using Current.Api.Entities;
using Current.Api.Common.Enums;

namespace Current.Api.Mappings;

public static class PaymentMappings
{
    public static PaymentReceiptResponse ToReceiptResponse(
        this Transaction transaction,
        Account recipientAccount,
        User recipientUser)
    {
        return new PaymentReceiptResponse
        {
            TransactionId = transaction.Id,
            FromAccountId = transaction.FromAccountId,
            RecipientAccountId = recipientAccount.Id,
            RecipientAccountName = recipientAccount.Name,
            RecipientName = $"{recipientUser.FirstName} {recipientUser.LastName}".Trim(),
            RecipientEmail = recipientUser.Email,
            Amount = transaction.Amount,
            Currency = recipientAccount.Currency,
            Reference = transaction.Reference,
            Status = transaction.Status,
            CreatedAt = transaction.CreatedAt
        };
    }

    public static PaymentHistoryItemResponse ToHistoryItemResponse(
        this Transaction transaction,
        Account fromAccount,
        User senderUser,
        Account toAccount,
        User recipientUser,
        PaymentDirection direction)
    {
        return new PaymentHistoryItemResponse
        {
            TransactionId = transaction.Id,
            Direction = direction,
            FromAccountId = fromAccount.Id,
            ToAccountId = toAccount.Id,
            SenderName = $"{senderUser.FirstName} {senderUser.LastName}".Trim(),
            SenderEmail = senderUser.Email,
            RecipientName = $"{recipientUser.FirstName} {recipientUser.LastName}".Trim(),
            RecipientEmail = recipientUser.Email,
            RecipientAccountName = toAccount.Name,
            Amount = transaction.Amount,
            Currency = fromAccount.Currency,
            Reference = transaction.Reference,
            Status = transaction.Status,
            CreatedAt = transaction.CreatedAt
        };
    }
}
