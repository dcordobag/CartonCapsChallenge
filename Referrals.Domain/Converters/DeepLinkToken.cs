using Referrals.Domain.Exceptions;

namespace Referrals.Domain.Converters
{

    /// <summary>
    /// Deep Link Token
    /// </summary>
    public readonly record struct DeepLinkToken(string Value)
    {
        /// <summary>
        /// Token ToDeepLinkToken
        /// </summary>
        public static DeepLinkToken ToDeepLinkToken(string value)
        {
            value = (value ?? string.Empty).Trim();

            // Opaque token: allow vendor-style strings, but keep a sane length for logs.
            if (value.Length < 8 || value.Length > 128)
                throw new DomainException("invalid_token", "Deep link token is invalid.");

            return new DeepLinkToken(value);
        }
    }

}
