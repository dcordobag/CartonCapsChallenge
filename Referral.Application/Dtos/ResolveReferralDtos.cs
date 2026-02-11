namespace Referrals.Application.Dtos
{
    using System.Collections.Generic;

    /// <summary>
    /// API Problem DTO used to serialize problems
    /// </summary>
    public sealed record ApiProblemDto(int? Status, string Title, string Type, string? Detail, string? Instance, Dictionary<string, object?>? Extensions);

    /// <summary>
    /// Health response DTO used by the /health endpoint.
    /// </summary>
    public sealed record HealthResponseDto(string Status);

    /// <summary>
    /// Resolve Referral Request Dto
    /// </summary>
    public sealed record ResolveReferralRequestDto(string Token, DeviceInfoDto Device);

    /// <summary>
    /// Device Info Dto
    /// </summary>
    public sealed record DeviceInfoDto(string DeviceId, string Platform, string AppVersion);

    /// <summary>
    /// Resolve Referral Response Dto
    /// </summary>
    public sealed record ResolveReferralResponseDto(
        bool IsReferred,
        string OnboardingVariant,
        string? ReferralCode,
        ReferrerInfoDto? Referrer,
        string? Destination
    );

    /// <summary>
    /// Referrer Info Dto
    /// </summary>
    public sealed record ReferrerInfoDto(string DisplayName, string? SchoolName);

    /// <summary>
    /// Accepted response DTO for endpoints that previously returned anonymous { accepted = true } objects.
    /// </summary>
    public sealed record AcceptedResponseDto(bool Accepted);
}

