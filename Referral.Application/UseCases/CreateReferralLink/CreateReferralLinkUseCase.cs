using Referrals.Application.Abstractions;
using Referrals.Application.Common;
using Referrals.Application.Dtos;
using Referrals.Domain.Converters;
using Referrals.Domain.Entities;
using Referrals.Domain.Enums;

namespace Referrals.Application.UseCases.CreateReferralLink
{
    /// <summary>
    /// Create Referral Link Use Case
    /// </summary>
    /// <remarks>
    /// Create Referral Link Use Case.
    /// </remarks>
    public sealed class CreateReferralLinkUseCase(
        IDeepLinkVendorClient _vendor,
        IReferralLinkRepository _links,
        IReferralRepository _referrals,
        IRateLimiter _rateLimiter)
    {

        /// <summary>
        /// Creates a referral link for the current user. 
        /// The link is tied to the user's referral code and can be used to track referrals 
        /// from different channels and campaigns.
        /// </summary>
        public async Task<CreateReferralLinkResponseDto> ExecuteAsync(
            string currentUserId,
            string currentUsersReferralCode,
            CreateReferralLinkRequestDto request,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
                throw new AppException("unauthorized", "Missing current user.");

            var requestedCode = ReferralCode.ToReferralCode(request.ReferralCode);

            // Prevent a user from generating links for someone else.
            if (!string.Equals(currentUsersReferralCode, requestedCode.Value, StringComparison.OrdinalIgnoreCase))
                throw new AppException("referral_code_mismatch", "Referral code does not belong to the current user.");

            var channel = ParseChannel(request.Channel);

            // Abuse mitigation (We would like to control how meny links a user can generate per X time):
            // - 5 links generations per minute per user (In a real project we can implement stronger limits).
            if (!_rateLimiter.TryConsume($"create_link:{currentUserId}", permits: 1))
                throw new AppException("rate_limited", "Too many requests. Please try again later.");

            // With te locale, campaign and destination parameters, we can support different variations of the referral flow.
            // For example, we can have different campaigns for "invite friends" vs "share on social media", and different destinations for "signup" vs "app download".
            // This allows us to track the effectiveness of different channels and campaigns.
            var locale = string.IsNullOrWhiteSpace(request.Locale) ? "en-US" : request.Locale.Trim();
            var campaign = string.IsNullOrWhiteSpace(request.Campaign) ? "invite_friends" : request.Campaign.Trim();
            var destination = string.IsNullOrWhiteSpace(request.Destination) ? "signup" : request.Destination.Trim();

            (DeepLinkToken token, string url, DateTimeOffset expiresAt) =
                await _vendor.CreateDeferredDeepLinkAsync(requestedCode, channel, campaign, destination, locale, ct);

            var link = new ReferralLink
            {
                Id = IdGenerator.NewLinkId(),
                ReferrerUserId = currentUserId,
                ReferralCode = requestedCode,
                Channel = channel,
                Token = token,
                Url = url,
                Campaign = campaign,
                Destination = destination,
                CreatedAt = DateTimeOffset.UtcNow,
                ExpiresAt = expiresAt
            };

            await _links.SaveAsync(link, ct);

            // Create a referral row tied to the link. Initially we don't know who the referee is.
            var referral = new Referral
            {
                Id = IdGenerator.NewReferralId(),
                ReferrerUserId = currentUserId,
                LinkId = link.Id,
                Channel = channel,
                Status = ReferralStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
                LastEventAt = DateTimeOffset.UtcNow,
                DisplayName = null
            };

            await _referrals.SaveAsync(referral, ct);

            var templates = ShareTemplateFactory.Build(requestedCode.Value, url);

            return new CreateReferralLinkResponseDto(
                LinkId: link.Id,
                Token: token.Value,
                Url: url,
                ExpiresAt: expiresAt,
                ShareTemplates: templates
            );
        }

        /// <summary>
        /// Parse Channel.
        /// </summary>
        private static ReferralChannel ParseChannel(string raw)
        {
            raw = (raw ?? string.Empty).Trim().ToLowerInvariant();

            return raw switch
            {
                "sms" => ReferralChannel.Sms,
                "text" => ReferralChannel.Sms,
                "email" => ReferralChannel.Email,
                "share" => ReferralChannel.Share,
                "copy" => ReferralChannel.Copy,
                _ => throw new AppException("invalid_request", $"Unknown channel '{raw}'.")
            };
        }

        /// <summary>
        /// Share Template Factory
        /// </summary>
        private static class ShareTemplateFactory
        {
            /// <summary>
            /// Build.
            /// </summary>
            public static ShareTemplatesDto Build(string referralCode, string url)
            {
                // Mimic the UI mocks: include a friendly message and a link.
                var sms = $"Hi! Join me in earning money for our school by using the Carton Caps app. " +
                          $"It's an easy way to make a difference. Use the link below to download the Carton Caps app: {url}";

                var emailSubject = "You're invited to try the Carton Caps app!";

                var emailBody =
                    "Hey!\n\n" +
                    "Join me in earning cash for our school by using the Carton Caps app. " +
                    "It's an easy way to make a difference. All you have to do is buy Carton Caps participating products (like Cheerios!) and scan your grocery receipt. " +
                    "Carton Caps are worth $.10 each and they add up fast! Twice a year, our school receives a check to help pay for whatever we need - equipment, supplies or experiences the kids love!\n\n" +
                    $"Download the Carton Caps app here: {url}";

                return new ShareTemplatesDto(sms, emailSubject, emailBody);
            }
        }
    }
}
