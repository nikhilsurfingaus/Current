namespace Current.Api.DTOs.Analytics;

public class MonthlySummaryResponse
{
    public int TransactionCount { get; set; }

    public decimal Income { get; set; }

    public decimal Expenses { get; set; }

    public decimal Transfers { get; set; }

    public decimal AverageTransactionAmount { get; set; }

    public decimal LargestExpense { get; set; }

    public decimal LargestIncome { get; set; }
}
