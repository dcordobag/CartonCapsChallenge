namespace Referrals.Application.Dtos
{
    /// <summary>
    /// Referral List Item Dto
    /// </summary>
    public sealed record ReferralListItemDto(
        string ReferralId,
        DateTimeOffset CreatedAt,
        string Status,
        string? DisplayName,
        DateTimeOffset LastEventAt,
        string Channel
    );

    /// <summary>
    /// Referral List Response Dto
    /// </summary>
    public sealed record ReferralListResponseDto(IReadOnlyList<ReferralListItemDto> Items, string? NextCursor);

    /// <summary>
    /// Referral Summary Dto
    /// </summary>
    public sealed record ReferralSummaryDto(int Total, int Pending, int Installed, int Complete, int Rewarded);

}
