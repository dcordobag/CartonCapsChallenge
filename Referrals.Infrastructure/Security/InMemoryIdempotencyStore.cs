using Referrals.Application.Abstractions;
using System.Collections.Concurrent;

namespace Referrals.Infrastructure.Security
{

    /// <summary>
    /// In Memory Idempotency Store
    /// </summary>
    public sealed class InMemoryIdempotencyStore : IIdempotencyStore
    {
        private sealed record Entry(string RequestHash, string ResponseJson);

        private readonly ConcurrentDictionary<string, Entry> _entries = new();

        private static string Key(string route, string key) => $"{route}::{key}";

        /// <summary>
        /// Find a cached referral link
        /// </summary>
        public Task<string?> TryGetAsync(string route, string key, CancellationToken ct)
        {
            if (_entries.TryGetValue(Key(route, key), out var entry))
                return Task.FromResult<string?>(entry.ResponseJson);

            return Task.FromResult<string?>(null);
        }

        /// <summary>
        /// Cache a referral link
        /// </summary>
        public Task SaveAsync(string route, string key, string requestHash, string responseJson, CancellationToken ct)
        {
            _entries[Key(route, key)] = new Entry(requestHash, responseJson);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Validate whether the incoming request is consistent with the cached request
        /// </summary>
        public Task<bool> IsConsistentAsync(string route, string key, string requestHash, CancellationToken ct)
        {
            if (!_entries.TryGetValue(Key(route, key), out var entry))
                return Task.FromResult(true);

            return Task.FromResult(string.Equals(entry.RequestHash, requestHash, StringComparison.Ordinal));
        }
    }

}
