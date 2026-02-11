namespace Referrals.Application.Dtos
{

    /// <summary>
    /// Vendor Event Dto
    /// </summary>
    public sealed record VendorEventDto(
        string EventType,
        string Token,
        DateTimeOffset OccurredAt,
        string DeviceIdHash,
        Dictionary<string, string>? Metadata
    );

}
