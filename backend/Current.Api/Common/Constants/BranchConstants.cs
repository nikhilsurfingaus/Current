namespace Current.Api.Common.Constants;

public static class BranchConstants
{
    public static readonly Guid SystemUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid HqBranchId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public static readonly Guid HqTreasuryAccountId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public const string HqBranchName = "Current HQ";

    public const string HqBranchCode = "HQ";

    public const string HqTreasuryAccountName = "Current HQ Treasury";

    public const string SystemUserEmail = "branch-system@current.internal";

    public const string WelcomeCreditDescription = "Welcome credit from Current HQ";

    public const string BranchTopUpDescription = "Branch top-up from Current HQ";
}
