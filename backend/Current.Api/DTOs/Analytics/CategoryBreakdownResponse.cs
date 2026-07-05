using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Analytics;

public class CategoryBreakdownItem
{
    public TransactionCategory Category { get; set; }

    public decimal Amount { get; set; }

    public decimal Percent { get; set; }
}

public class CategoryBreakdownResponse
{
    public IReadOnlyList<CategoryBreakdownItem> Categories { get; set; } = Array.Empty<CategoryBreakdownItem>();
}
