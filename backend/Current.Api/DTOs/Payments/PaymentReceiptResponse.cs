using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Payments;

public class PaymentReceiptResponse
{
    public Guid TransactionId { get; set; }

    public Guid FromAccountId { get; set; }

    public Guid RecipientAccountId { get; set; }

    public required string RecipientAccountName { get; set; }

    public required string RecipientName { get; set; }

    public string? RecipientEmail { get; set; }

    public string RecipientBsb { get; set; } = string.Empty;

    public string RecipientAccountNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public required string Currency { get; set; }

    public string? Reference { get; set; }

    public TransactionStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}
