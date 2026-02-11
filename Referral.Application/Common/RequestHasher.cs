using System.Security.Cryptography;
using System.Text;

namespace Referrals.Application.Common
{
    /// <summary>
    /// Request Hasher
    /// </summary>
    public static class RequestHasher
    {
        /// <summary>
        /// Generates a SHA256 hash for every request. 
        /// We will use it to create idempotency keys
        /// </summary>
        public static string Sha256(object value)
        {
            // For mock purposes we intentionally keep hashing simple and based on ToString().
            // In a real implementation,
            // we would need to be careful about how we generate the hash to
            // avoid collisions and ensure that semantically
            // identical requests produce the same hash even if their string representations differ.
            var text = value?.ToString() ?? string.Empty;
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
            return Convert.ToHexString(bytes);
        }

    }
}
