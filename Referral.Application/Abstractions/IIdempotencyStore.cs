namespace Referrals.Application.Abstractions
{

    /// <summary>
    /// I Idempotency Store
    /// </summary>
    public interface IIdempotencyStore
    {
        /// <summary>
        /// Returns a previously cached response for the same key+route, or null.
        /// </summary>
        Task<string?> TryGetAsync(string route, string key, CancellationToken ct);

        /// <summary>
        /// Stores the serialized response for future identical requests.
        /// </summary>
        Task SaveAsync(string route, string key, string requestHash, string responseJson, CancellationToken ct);

        /// <summary>
        /// If a key exists with a different request hash, return false to indicate conflict.
        /// </summary>
        Task<bool> IsConsistentAsync(string route, string key, string requestHash, CancellationToken ct);
    }
}
