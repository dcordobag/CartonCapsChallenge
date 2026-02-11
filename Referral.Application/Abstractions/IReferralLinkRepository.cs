using Referrals.Domain.Converters;
using Referrals.Domain.Entities;

namespace Referrals.Application.Abstractions
{
    /// <summary>
    /// I Referral Link Repository
    /// </summary>
    public interface IReferralLinkRepository
    {
        Task SaveAsync(ReferralLink link, CancellationToken ct);
        Task<ReferralLink?> FindByTokenAsync(DeepLinkToken token, CancellationToken ct);
        Task<ReferralLink?> FindByIdAsync(string linkId, CancellationToken ct);
    }

}
