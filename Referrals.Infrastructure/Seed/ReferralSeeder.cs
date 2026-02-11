using Referrals.Application.Abstractions;
using Referrals.Domain.Converters;
using Referrals.Domain.Entities;
using Referrals.Domain.Enums;

namespace Referrals.Infrastructure.Seed
{

    /// <summary>
    /// Referral Seeder
    /// </summary>
    /// <remarks>
    /// Referral Seeder.
    /// </remarks>
    public sealed class ReferralSeeder(IReferralLinkRepository _links, IReferralRepository _referrals)
    {

        /// <summary>
        /// Seed some referrals for user_123 so the Invite Friends screen has realistic data.
        /// </summary>
        public async Task SeedAsync(CancellationToken ct)
        {
            // Safe to call multiple times; only seeds if empty.
            var existing = await _referrals.CountByReferrerAsync("user_123", ct);
            if (existing.Values.Sum() > 0) return;

            var now = DateTime.UtcNow;

            // Create three completed referrals
            for (var i = 0; i < 3; i++)
            {
                var token = new DeepLinkToken($"dl_seed_{i}_{Guid.NewGuid():N}");
                var link = new ReferralLink
                {
                    Id = $"rl_seed_{i}",
                    ReferrerUserId = "user_123",
                    ReferralCode = ReferralCode.ToReferralCode("XY7G4D"),
                    Channel = ReferralChannel.Sms,
                    Token = token,
                    Url = $"https://cartoncaps.link/{Uri.EscapeDataString(token.Value)}?referral_code=XY7G4D",
                    Campaign = "invite_friends",
                    Destination = "signup",
                    CreatedAt = now.AddDays(-10 + i),
                    ExpiresAt = now.AddDays(20)
                };

                await _links.SaveAsync(link, ct);

                var referral = new Referral
                {
                    Id = $"ref_seed_{i}",
                    ReferrerUserId = "user_123",
                    LinkId = link.Id,
                    Channel = ReferralChannel.Sms,
                    Status = ReferralStatus.Complete,
                    DisplayName = i switch
                    {
                        0 => "Jenny S.",
                        1 => "Archer K.",
                        _ => "Helen Y."
                    },
                    CreatedAt = now.AddDays(-10 + i),
                    LastEventAt = now.AddDays(-9 + i)
                };

                await _referrals.SaveAsync(referral, ct);
            }
        }
    }

}
