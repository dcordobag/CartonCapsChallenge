using Referrals.Application.Abstractions;
using Referrals.Domain.Entities;
using Referrals.Domain.Enums;
using System.Text;

namespace Referrals.Infrastructure.Persistence
{

    /// <summary>
    /// In Memory Referral Repository
    /// </summary>
    public sealed class InMemoryReferralRepository(InMemoryStore _store) : IReferralRepository
    {

        /// <summary>
        /// Save Referral.
        /// </summary>
        public Task SaveAsync(Referral referral, CancellationToken ct)
        {
            _store.ReferralsById[referral.Id] = referral;
            _store.ReferralIdByLinkId[referral.LinkId] = referral.Id;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Find By Link Id.
        /// </summary>
        public Task<Referral?> FindByLinkIdAsync(string linkId, CancellationToken ct)
        {
            if (!_store.ReferralIdByLinkId.TryGetValue(linkId, out var referralId))
                return Task.FromResult<Referral?>(null);

            _store.ReferralsById.TryGetValue(referralId, out var referral);
            return Task.FromResult(referral);
        }

        /// <summary>
        /// Lists referrals for a referrer with optional status filtering and cursor-based pagination.
        /// </summary>
        public Task<(IReadOnlyList<Referral> Items, string? NextCursor)> ListByReferrerAsync(
            string referrerUserId,
            ReferralStatus? status,
            int limit,
            string? cursor,
            CancellationToken ct)
        {
            var all = _store.ReferralsById.Values
                .Where(r => r.ReferrerUserId == referrerUserId)
                .Where(r => status is null || r.Status == status.Value)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            var offset = DecodeCursor(cursor);
            var page = all.Skip(offset).Take(limit).ToList();

            var nextOffset = offset + page.Count;
            string? nextCursor = nextOffset < all.Count ? EncodeCursor(nextOffset) : null;

            return Task.FromResult(((IReadOnlyList<Referral>)page, nextCursor));
        }

        /// <summary>
        /// Count By Referrer.
        /// </summary>
        public Task<Dictionary<ReferralStatus, int>> CountByReferrerAsync(string referrerUserId, CancellationToken ct)
        {
            var dict = _store.ReferralsById.Values
                .Where(r => r.ReferrerUserId == referrerUserId)
                .GroupBy(r => r.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            return Task.FromResult(dict);
        }

        /// <summary>
        /// Encode Cursor.
        /// </summary>
        private static string EncodeCursor(int offset) =>
            Convert.ToBase64String(Encoding.UTF8.GetBytes(offset.ToString()));

        /// <summary>
        /// Decode Cursor.
        /// </summary>
        private static int DecodeCursor(string? cursor)
        {
            if (string.IsNullOrWhiteSpace(cursor))
                return 0;

            try
            {
                var text = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
                return int.TryParse(text, out var offset) ? Math.Max(0, offset) : 0;
            }
            catch
            {
                return 0;
            }
        }
    }

}
