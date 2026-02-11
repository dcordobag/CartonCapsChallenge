using Referrals.Domain.Enums;

namespace Referrals.Domain.Entities
{

    /// <summary>
    /// Referral
    /// </summary>
    public sealed class Referral
    {
        public string Id { get; init; } = default!;
        public string ReferrerUserId { get; init; } = default!;
        public string LinkId { get; init; } = default!;

        // Unknown until completion (registration service will associate later).
        public string? RefereeUserId { get; set; }

        public ReferralStatus Status { get; set; } = ReferralStatus.Pending;

        // For UI list display.
        public string? DisplayName { get; set; }
        public ReferralChannel Channel { get; init; }

        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset LastEventAt { get; set; }
    }
}
