using Referrals.Application.Abstractions;

namespace Referrals.Infrastructure.Profiles
{

    /// <summary>
    /// Fake Referrer Info Provider
    /// </summary>
    public sealed class FakeReferrerInfoProvider : IReferrerInfoProvider
    {
        // 
        /// <summary>
        /// Mock user directory for referrer profile details.
        /// </summary>
        private static readonly Dictionary<string, (string Name, string School)> Users = new()
        {
            ["user_123"] = ("Darri Cordoba", "Carton Caps"),
            ["user_456"] = ("Juan Torres", "Carton Caps"),
            ["user_789"] = ("Pepito Perez", "Carton Caps")
        };

        /// <summary>
        /// We return a friendly referrer identity to display during onboarding.
        /// </summary>
        public Task<(string DisplayName, string? SchoolName)> GetReferrerInfoAsync(string referrerUserId, CancellationToken ct)
        {
            if (Users.TryGetValue(referrerUserId, out var v))
                return Task.FromResult<(string, string?)>((v.Name, v.School));

            return Task.FromResult<(string, string?)>(("Single User", null));
        }
    }
}
