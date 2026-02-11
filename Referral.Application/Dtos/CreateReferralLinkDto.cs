namespace Referrals.Application.Dtos
{
    #region Requests
    /// <summary>
    /// Create Referral Link Request Dto
    /// </summary>
    public sealed record CreateReferralLinkRequestDto(
        string ReferralCode,
        string Channel,
        string Locale,
        string Campaign,
        string Destination,
        ClientInfoDto Client
    );

    /// <summary>
    /// Client Info Dto
    /// </summary>
    public sealed record ClientInfoDto(string Platform, string AppVersion);


    #endregion

    #region Responses

    /// <summary>
    /// Create Referral Link Response Dto
    /// </summary>
    public sealed record CreateReferralLinkResponseDto(
        string LinkId,
        string Token,
        string Url,
        DateTimeOffset ExpiresAt,
        ShareTemplatesDto ShareTemplates
    );


    /// <summary>
    /// Share Templates Dto
    /// </summary>
    public sealed record ShareTemplatesDto(
        string Sms,
        string EmailSubject,
        string EmailBody
    );

    #endregion

}
