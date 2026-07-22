using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Loans;

public class LoanAdminResponse : LoanResponse
{
    public string BorrowerEmail { get; set; } = string.Empty;

    public string BorrowerName { get; set; } = string.Empty;
}
