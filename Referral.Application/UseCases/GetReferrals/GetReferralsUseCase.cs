using Referrals.Application.Abstractions;
using Referrals.Application.Common;
using Referrals.Application.Dtos;
using Referrals.Domain.Enums;

namespace Referrals.Application.UseCases.GetReferrals
{
    /// <summary>
    /// Get My Referrals Use Case
    /// </summary>
    public sealed class GetReferralsUseCase(IReferralRepository _referrals)
    {
        /// <summary>
        /// Get Referrals to a referrer with optional status filter and pagination.
        /// </summary>
        public async Task<ReferralListResponseDto> ExecuteAsync(
            string currentUserId,
            string? status,
            int limit,
            string? cursor,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new AppException("unauthorized", "Missing current user.");

            limit = Math.Clamp(limit <= 0 ? 20 : limit, 1, 100);

            ReferralStatus? parsedStatus = null;
            if (!string.IsNullOrWhiteSpace(status))
                parsedStatus = ParseStatus(status!);

            var (items, nextCursor) = await _referrals.ListByReferrerAsync(currentUserId, parsedStatus, limit, cursor, ct);

            var dtoItems = items.Select(r => new ReferralListItemDto(
                ReferralId: r.Id,
                CreatedAt: r.CreatedAt,
                Status: ToApiStatus(r.Status),
                DisplayName: r.DisplayName,
                LastEventAt: r.LastEventAt,
                Channel: r.Channel.ToString().ToLowerInvariant()
            )).ToList();

            return new ReferralListResponseDto(dtoItems, nextCursor);
        }

        /// <summary>
        /// Parse Status.
        /// </summary>
        private static ReferralStatus ParseStatus(string status)
        {
            status = status.Trim().ToLowerInvariant();

            return status switch
            {
                "pending" => ReferralStatus.Pending,
                "installed" => ReferralStatus.Installed,
                "complete" => ReferralStatus.Complete,
                "rewarded" => ReferralStatus.Rewarded,
                _ => throw new AppException("invalid_request", $"Unknown status '{status}'.")
            };
        }

        /// <summary>
        /// To Api Status.
        /// </summary>
        private static string ToApiStatus(ReferralStatus status) =>
            status switch
            {
                ReferralStatus.Pending => "pending",
                ReferralStatus.Installed => "installed",
                ReferralStatus.Complete => "complete",
                ReferralStatus.Rewarded => "rewarded",
                _ => "pending"
            };
    }

}
