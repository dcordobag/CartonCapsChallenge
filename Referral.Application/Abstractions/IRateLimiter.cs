namespace Referrals.Application.Abstractions
{
    /// <summary>
    /// I Rate Limiter
    /// </summary>
    public interface IRateLimiter
    {
        /// <summary>
        /// Returns false if the request should be rejected due to throttling.
        /// </summary>
        bool TryConsume(string key, int permits = 1);
    }

}
