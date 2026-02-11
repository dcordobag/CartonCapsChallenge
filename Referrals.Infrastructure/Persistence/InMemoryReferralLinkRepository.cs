using Referrals.Application.Abstractions;
using Referrals.Domain.Converters;
using Referrals.Domain.Entities;

namespace Referrals.Infrastructure.Persistence
{

    /// <summary>
    /// In Memory Referral Link Repository
    /// </summary>
    /// <remarks>
    /// In Memory Referral Link Repository.
    /// </remarks>
    public sealed class InMemoryReferralLinkRepository(InMemoryStore _store) : IReferralLinkRepository
    {

        /// <summary>
        /// Save Referral Link.
        /// </summary>
        public Task SaveAsync(ReferralLink link, CancellationToken ct)
        {
            _store.LinksById[link.Id] = link;
            _store.LinkIdByToken[link.Token.Value] = link.Id;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Find By Token.
        /// </summary>
        public Task<ReferralLink?> FindByTokenAsync(DeepLinkToken token, CancellationToken ct)
        {
            if (!_store.LinkIdByToken.TryGetValue(token.Value, out var linkId))
                return Task.FromResult<ReferralLink?>(null);

            _store.LinksById.TryGetValue(linkId, out var link);
            return Task.FromResult(link);
        }

        /// <summary>
        /// Find By Id.
        /// </summary>
        public Task<ReferralLink?> FindByIdAsync(string linkId, CancellationToken ct)
        {
            _store.LinksById.TryGetValue(linkId, out var link);
            return Task.FromResult(link);
        }
    }

}
