using Referrals.Application.Abstractions;
using System.Collections.Concurrent;

namespace Referrals.Infrastructure.Security
{
    /// <summary>
    /// This is the in-memory limiter to demonstrate abuse mitigation.
    /// </summary>
    /// <remarks>
    /// In Memory Rate Limiter.
    /// </remarks>
    public sealed class InMemoryRateLimiter(int _maxEventsPerWindow = 5, TimeSpan? window = null) : IRateLimiter
    {
        private readonly ConcurrentDictionary<string, Queue<DateTimeOffset>> _events = new();

        private readonly TimeSpan _window = window ?? TimeSpan.FromMinutes(1);

        /// <summary>
        /// With this method we validathe if the user can continue making request or not in X timeframe. 
        /// For example, we can allow 5 requests per minute per user. 
        /// This is a simple strategy to prevent abuse of the referral system.
        /// </summary>
        public bool TryConsume(string key, int permits = 1)
        {
            if (permits <= 0) return true;

            var now = DateTimeOffset.UtcNow;
            var queue = _events.GetOrAdd(key, _ => new Queue<DateTimeOffset>());

            lock (queue)
            {
                // prune old events
                while (queue.Count > 0 && now - queue.Peek() > _window)
                    queue.Dequeue();

                if (queue.Count + permits > _maxEventsPerWindow)
                    return false;

                for (var i = 0; i < permits; i++)
                    queue.Enqueue(now);

                return true;
            }
        }
    }
}
