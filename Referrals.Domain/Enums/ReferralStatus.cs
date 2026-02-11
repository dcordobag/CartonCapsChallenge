namespace Referrals.Domain.Enums
{

    /// <summary>
    /// Referral Status
    /// </summary>
    public enum ReferralStatus
    {
        /// <summary>Link was created / shared, but no install attributed yet.</summary>
        Pending = 0,

        /// <summary>Vendor attributed an install or first-open to the invite.</summary>
        Installed = 1,

        /// <summary>Referee completed registration and redeemed referral during signup (handled by existing service).</summary>
        Complete = 2,

        /// <summary>Reward issued.</summary>
        Rewarded = 3
    }

}
