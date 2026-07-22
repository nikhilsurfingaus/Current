namespace Current.Api.Entities;

public class Branch
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public Guid TreasuryAccountId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Account TreasuryAccount { get; set; } = null!;
}
