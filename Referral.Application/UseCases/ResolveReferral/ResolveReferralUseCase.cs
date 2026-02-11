using Referrals.Application.Abstractions;
using Referrals.Application.Common;
using Referrals.Application.Dtos;
using Referrals.Domain.Converters;

namespace Referrals.Application.UseCases.ResolveReferral
{

    /// <summary>
    /// Resolve Referral Use Case
    /// </summary>
    /// <remarks>
    /// Resolve Referral Use Case.
    /// </remarks>
    public sealed class ResolveReferralUseCase(
        IReferralLinkRepository _links,
        IReferrerInfoProvider _referrerInfo,
        IRateLimiter _rateLimiter)
    {

        /// <summary>
        /// Resolves a referral token to get referral details such as the referrer information and the destination.
        /// </summary>
        public async Task<ResolveReferralResponseDto> ExecuteAsync(ResolveReferralRequestDto request, CancellationToken ct)
        {
            // Protect from multiples requests with the same device id.
            // This is a simple rate limit strategy to prevent abuse of the referral system.
            var deviceKey = string.IsNullOrWhiteSpace(request.Device.DeviceId) ? "unknown" : request.Device.DeviceId;
            if (!_rateLimiter.TryConsume($"resolve:{deviceKey}", permits: 1))
                throw new AppException("rate_limited", "Too many requests. Please try again later.");

            var token = DeepLinkToken.ToDeepLinkToken(request.Token);

            var link = await _links.FindByTokenAsync(token, ct);
            if (link is null)
                throw new AppException("token_not_found", "Referral token was not found.");

            if (link.ExpiresAt <= DateTime.UtcNow)
                throw new AppException("token_expired", "Referral token is expired.");

            var (displayName, schoolName) = await _referrerInfo.GetReferrerInfoAsync(link.ReferrerUserId, ct);

            return new ResolveReferralResponseDto(
                IsReferred: true,
                OnboardingVariant: "referred",
                ReferralCode: link.ReferralCode.Value,
                Referrer: new ReferrerInfoDto(displayName, schoolName),
                Destination: link.Destination
            );
        }
    }

}
