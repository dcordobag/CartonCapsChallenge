using Referrals.Application.Dtos;
using System.Text.Json.Serialization;

namespace Referrals.Api.Serialization
{
    /* 
     * JSON serialization context for DTOs used by the API endpoints.
    * This ensures JsonTypeInfo metadata is available at runtime (and for OpenAPI) when
    * System.Text.Json source generation is enabled.
    */
    [JsonSerializable(typeof(CreateReferralLinkRequestDto))]
    [JsonSerializable(typeof(CreateReferralLinkResponseDto))]
    [JsonSerializable(typeof(ShareTemplatesDto))]
    [JsonSerializable(typeof(ClientInfoDto))]

    [JsonSerializable(typeof(ReferralListResponseDto))]
    [JsonSerializable(typeof(ReferralListItemDto))]
    [JsonSerializable(typeof(ReferralSummaryDto))]

    [JsonSerializable(typeof(ResolveReferralRequestDto))]
    [JsonSerializable(typeof(ResolveReferralResponseDto))]
    [JsonSerializable(typeof(DeviceInfoDto))]
    [JsonSerializable(typeof(ReferrerInfoDto))]

    [JsonSerializable(typeof(VendorEventDto))]
    [JsonSerializable(typeof(HealthResponseDto))]
    [JsonSerializable(typeof(ApiProblemDto))]
    [JsonSerializable(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails))]
    [JsonSerializable(typeof(AcceptedResponseDto))]

    public partial class ReferralsJsonContext : JsonSerializerContext
    {
    }
}