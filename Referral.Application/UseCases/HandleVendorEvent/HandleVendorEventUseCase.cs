using Referrals.Application.Abstractions;
using Referrals.Application.Common;
using Referrals.Application.Dtos;
using Referrals.Domain.Converters;
using Referrals.Domain.Enums;

namespace Referrals.Application.UseCases.HandleVendorEvent
{

    /// <summary>
    /// Handle Vendor Event Use Case
    /// </summary>
    public sealed class HandleVendorEventUseCase(IReferralLinkRepository _links, IReferralRepository _referrals)
    {
        /// <summary>
        ///  Manages the referral status based on the event type received from the vendor. 
        ///  It updates the referral status accordingly and saves the changes to the repository.
        /// </summary>
        public async Task ExecuteAsync(VendorEventDto evt, CancellationToken ct)
        {
            var token = DeepLinkToken.ToDeepLinkToken(evt.Token);

            var link = await _links.FindByTokenAsync(token, ct) ?? throw new AppException("token_not_found", "Referral token was not found.");
            var referral = await _referrals.FindByLinkIdAsync(link.Id, ct) ?? throw new AppException("referral_not_found", "Referral was not found.");
            var eventType = (evt.EventType ?? string.Empty).Trim().ToLowerInvariant();

            switch (eventType)
            {
                case "install":
                case "open":
                    referral.Status = referral.Status == ReferralStatus.Complete || referral.Status == ReferralStatus.Rewarded
                        ? referral.Status
                        : ReferralStatus.Installed;
                    break;

                case "complete":
                    referral.Status = ReferralStatus.Complete;
                    // This name would be setted by the vendor, but if it's not provided, we can set a default name for the referral.
                    referral.DisplayName ??= "New friend";
                    break;

                case "rewarded":
                    referral.Status = ReferralStatus.Rewarded;
                    break;

                default:
                    throw new AppException("invalid_request", $"Unknown eventType '{evt.EventType}'.");
            }

            referral.LastEventAt = evt.OccurredAt == default ? DateTime.UtcNow : evt.OccurredAt;

            await _referrals.SaveAsync(referral, ct);
        }
    }

}
