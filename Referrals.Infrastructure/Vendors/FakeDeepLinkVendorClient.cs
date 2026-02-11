using Referrals.Application.Abstractions;
using Referrals.Domain.Converters;
using Referrals.Domain.Enums;

namespace Referrals.Infrastructure.Vendors
{

    /// <summary>
    /// Fake Deep Link Vendor Client
    /// </summary>
    public sealed class FakeDeepLinkVendorClient : IDeepLinkVendorClient
    {
     
        /// <summary>
        /// Method to create a vendor-managed deferred deep link for the provided referral code.
        /// </summary>
        /// <remarks>
        /// This mock implementation will generate a URL in the format:
        /// <c>https://cartoncaps.link/{token}?referral_code={code}</c>.
        /// </remarks>
        public Task<(DeepLinkToken Token, string Url, DateTimeOffset ExpiresAt)> CreateDeferredDeepLinkAsync(
            ReferralCode referralCode,
            ReferralChannel channel,
            string campaign,
            string destination,
            string locale,
            CancellationToken ct)
        {
            // In a real projects the url will be stored in some configuration file or something.
            // We mimic a short-link domain and put an opaque token into the URL path.
            var token = new DeepLinkToken($"dl_{Guid.NewGuid():N}");
            var url = $"https://cartoncaps.link/{Uri.EscapeDataString(token.Value)}?referral_code={Uri.EscapeDataString(referralCode.Value)}";
            var expiresAt = DateTimeOffset.UtcNow.AddDays(30);

            return Task.FromResult((token, url, expiresAt));
        }
    }
}
