using Referrals.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace Referrals.Domain.Converters
{
    /// <summary>
    /// Referral Code
    /// </summary>
    public readonly record struct ReferralCode(string Value)
    {
        /// <summary>
        /// The codes should follow a known pattern
        /// </summary>
        private static readonly Regex Pattern = new("^[A-Z0-9]{4,10}$", RegexOptions.Compiled);

        /// <summary>
        /// Convert string to ReferralCode.
        /// </summary>
        public static ReferralCode ToReferralCode(string value)
        {
            value = (value ?? string.Empty).Trim().ToUpperInvariant();

            if (!Pattern.IsMatch(value))
                throw new DomainException("invalid_referral_code", "Referral code format is invalid.");

            return new ReferralCode(value);
        }
    }

}
