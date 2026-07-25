using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Payments;

public class PaymentHistoryItemResponse
{
    public Guid TransactionId { get; set; }

    public PaymentDirection Direction { get; set; }

    public Guid FromAccountId { get; set; }

    public Guid ToAccountId { get; set; }

    public required string SenderName { get; set; }

    public required string SenderEmail { get; set; }

    public required string RecipientName { get; set; }

    public string? RecipientEmail { get; set; }

    public string RecipientBsb { get; set; } = string.Empty;

    public string RecipientAccountNumber { get; set; } = string.Empty;

    public required string RecipientAccountName { get; set; }

    public decimal Amount { get; set; }

    public required string Currency { get; set; }

    public string? Reference { get; set; }

    public TransactionStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}
