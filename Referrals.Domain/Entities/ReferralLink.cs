using Referrals.Domain.Converters;
using Referrals.Domain.Enums;

namespace Referrals.Domain.Entities
{
    /// <summary>
    /// Referral Link
    /// </summary>
    public sealed class ReferralLink
    {
        public string Id { get; init; } = default!;
        public string ReferrerUserId { get; init; } = default!;
        public ReferralCode ReferralCode { get; init; }
        public ReferralChannel Channel { get; init; }
        public DeepLinkToken Token { get; init; }
        public string Url { get; init; } = default!;
        public string Campaign { get; init; } = "invite_friends";
        public string Destination { get; init; } = "signup";
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }
    }
}
