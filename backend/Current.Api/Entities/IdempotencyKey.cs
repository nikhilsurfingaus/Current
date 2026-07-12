namespace Current.Api.Entities;

public class IdempotencyKey
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Key { get; set; }

    public required string RequestHash { get; set; }

    public required string ResponseJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public User User { get; set; } = null!;
}
