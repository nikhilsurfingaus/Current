using Current.Api.DTOs.Branches;

namespace Current.Api.Interfaces;

public interface IBranchService
{
    Task<BranchTreasuryResponse> GetTreasuryAsync();

    Task<BranchDisbursementResponse> CreateDisbursementAsync(CreateBranchDisbursementRequest request);
}
