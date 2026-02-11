using Referrals.Domain.Entities;
using Referrals.Domain.Enums;

namespace Referrals.Application.Abstractions
{
    /// <summary>
    /// I Referral Repository
    /// </summary>
    public interface IReferralRepository
    {
        Task SaveAsync(Referral referral, CancellationToken ct);
        Task<(IReadOnlyList<Referral> Items, string? NextCursor)> ListByReferrerAsync(
            string referrerUserId,
            ReferralStatus? status,
            int limit,
            string? cursor,
            CancellationToken ct);

        Task<Dictionary<ReferralStatus, int>> CountByReferrerAsync(string referrerUserId, CancellationToken ct);
        Task<Referral?> FindByLinkIdAsync(string linkId, CancellationToken ct);
    }
}
