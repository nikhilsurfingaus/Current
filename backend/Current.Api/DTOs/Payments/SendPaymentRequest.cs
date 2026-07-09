namespace Current.Api.DTOs.Payments;

public class SendPaymentRequest
{
    public Guid FromAccountId { get; set; }

    public required string RecipientEmail { get; set; }

    public decimal Amount { get; set; }

    public string? Reference { get; set; }
}
