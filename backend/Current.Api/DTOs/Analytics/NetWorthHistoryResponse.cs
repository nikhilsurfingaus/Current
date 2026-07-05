namespace Current.Api.DTOs.Analytics;

public class NetWorthHistoryPoint
{
    public DateOnly Date { get; set; }

    public decimal Balance { get; set; }
}

public class NetWorthHistoryResponse
{
    public IReadOnlyList<NetWorthHistoryPoint> Points { get; set; } = Array.Empty<NetWorthHistoryPoint>();
}
