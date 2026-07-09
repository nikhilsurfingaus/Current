using Current.Api.DTOs.Payments;
using Current.Api.Entities;

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
}
