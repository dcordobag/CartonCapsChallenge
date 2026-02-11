namespace Referrals.Application.Common
{

    /// <summary>
    /// Id Generator
    /// </summary>
    public static class IdGenerator
    {
        /// <summary>
        /// New Link Id.
        /// </summary>
        public static string NewLinkId() => $"rl_{Guid.NewGuid():N}";
        /// <summary>
        /// New Referral Id.
        /// </summary>
        public static string NewReferralId() => $"ref_{Guid.NewGuid():N}";
    }

}
