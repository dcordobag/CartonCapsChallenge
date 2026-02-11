using Referrals.Application.Abstractions;
using Referrals.Application.Common;
using Referrals.Application.Dtos;
using Referrals.Domain.Enums;

namespace Referrals.Application.UseCases.GetReferralSummary
{

    /// <summary>
    /// Get Referral Summary Use Case
    /// </summary>
    /// <remarks>
    /// Get Referral Summary Use Case.
    /// </remarks>
    public sealed class GetReferralSummaryUseCase(IReferralRepository _referrals)
    {
        /// <summary>
        /// Get Referral Summary for the current user. 
        /// It returns the total count of referrals and the count for each status.
        /// </summary>
        public async Task<ReferralSummaryDto> ExecuteAsync(string currentUserId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new AppException("unauthorized", "Missing current user.");

            var counts = await _referrals.CountByReferrerAsync(currentUserId, ct);
            // In a real app we will probably do this from a database with a single query.
            int Get(ReferralStatus key) => counts.TryGetValue(key, out var result) ? result : 0;
            var pending = Get(ReferralStatus.Pending);
            var installed = Get(ReferralStatus.Installed);
            var complete = Get(ReferralStatus.Complete);
            var rewarded = Get(ReferralStatus.Rewarded);

            return new ReferralSummaryDto(
                Total: pending + installed + complete + rewarded,
                Pending: pending,
                Installed: installed,
                Complete: complete,
                Rewarded: rewarded
            );
        }
    }

}
