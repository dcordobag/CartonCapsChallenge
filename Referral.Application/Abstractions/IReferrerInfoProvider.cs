namespace Referrals.Application.Abstractions
{

    /// <summary>
    /// I Referrer Info Provider
    /// </summary>
    public interface IReferrerInfoProvider
    {
        Task<(string DisplayName, string? SchoolName)> GetReferrerInfoAsync(string referrerUserId, CancellationToken ct);
    }

}
