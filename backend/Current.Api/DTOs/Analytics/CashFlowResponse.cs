namespace Current.Api.DTOs.Analytics;

public class CashFlowMonthPoint
{
    public string Month { get; set; } = string.Empty;

    public decimal Income { get; set; }

    public decimal Expenses { get; set; }

    public decimal Net { get; set; }
}

public class CashFlowResponse
{
    public IReadOnlyList<CashFlowMonthPoint> Months { get; set; } = Array.Empty<CashFlowMonthPoint>();
}
