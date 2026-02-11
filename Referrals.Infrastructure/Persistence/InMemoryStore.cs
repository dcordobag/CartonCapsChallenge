using Referrals.Domain.Entities;
using System.Collections.Concurrent;

namespace Referrals.Infrastructure.Persistence
{
    /// <summary>
    /// In Memory Store
    /// </summary>
    public sealed class InMemoryStore
    {
        /// <summary>
        /// LinksById
        /// </summary>
        public ConcurrentDictionary<string, ReferralLink> LinksById { get; } = new();
        /// <summary>
        /// LinkIdByToken
        /// </summary>
        public ConcurrentDictionary<string, string> LinkIdByToken { get; } = new();

        /// <summary>
        /// ReferralsById
        /// </summary>
        public ConcurrentDictionary<string, Referral> ReferralsById { get; } = new();
        /// <summary>
        /// ReferralIdByLinkId
        /// </summary>
        public ConcurrentDictionary<string, string> ReferralIdByLinkId { get; } = new();
    }
}
