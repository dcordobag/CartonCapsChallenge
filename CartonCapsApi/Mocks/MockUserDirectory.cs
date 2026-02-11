namespace Referrals.Api.Mocks
{
    /// <summary>
    /// Mock User Directory
    /// </summary>
    public sealed class MockUserDirectory
    {
        // In a real system, the mobile app would fetch referralCode from the existing profile endpoint.
        // For the mock, we keep a stable mapping.
        /// <summary>
        /// New.
        /// </summary>
        private readonly Dictionary<string, string> _referralCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["user_123"] = "XY7G4D",
            ["user_456"] = "Q1W2E3",
            ["user_789"] = "A9B8C7"
        };

        /// <summary>
        /// Get Referral Code For.
        /// </summary>
        public string GetReferralCodeFor(string userId) =>
            _referralCodes.TryGetValue(userId, out var code) ? code : "UNKN0WN";
    }
}
