using Referrals.Domain.Converters;
using Referrals.Domain.Enums;

namespace Referrals.Application.Abstractions
{
    /// <summary>
    /// I Deep Link Vendor Client
    /// </summary>
    public interface IDeepLinkVendorClient
    {
        Task<(DeepLinkToken Token, string Url, DateTimeOffset ExpiresAt)> CreateDeferredDeepLinkAsync(
            ReferralCode referralCode,
            ReferralChannel channel,
            string campaign,
            string destination,
            string locale,
            CancellationToken ct);
    }

}
