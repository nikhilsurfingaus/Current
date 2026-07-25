namespace Current.Api.DTOs.Payments;

public class SendPaymentRequest
{
    public Guid FromAccountId { get; set; }

    public string? RecipientEmail { get; set; }

    public string? RecipientBsb { get; set; }

    public string? RecipientAccountNumber { get; set; }

    public decimal Amount { get; set; }

    public string? Reference { get; set; }
}
