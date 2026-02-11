using Microsoft.AspNetCore.Mvc;
using Referrals.Api.Mocks;
using Referrals.Application.Abstractions;
using Referrals.Application.Common;
using Referrals.Application.Dtos;
using Referrals.Application.UseCases.CreateReferralLink;
using Referrals.Application.UseCases.GetReferrals;
using Referrals.Application.UseCases.GetReferralSummary;
using Referrals.Application.UseCases.HandleVendorEvent;
using Referrals.Application.UseCases.ResolveReferral;
using System.Text.Json;
using Referrals.Api.Serialization;

namespace Referrals.Api.Endpoints
{
    /// <summary>
    /// To register the endpoints related to referrals
    /// </summary>
    public static class ReferralEndpoints
    {
        /*
         * Vendor signature header and secret for webhook validation: 
         * In real projects we should use a secure storage, keyvault, 
         * cyberark or something
         */
        private const string VendorSignatureHeader = "X-Vendor-Signature";
        private const string VendorSignatureSecret = "test_secret";

        /// <summary>
        /// Method to map the referral endpoints
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IEndpointRouteBuilder MapReferralEndpoints(this IEndpointRouteBuilder app)
        {
            var v1 = app.MapGroup("/v1");

            #region Methods with authentication
            //Group to handle authenticated endpoints
            var authenticatedGroup = v1.MapGroup("/referrals");

            // Method to create a new link
            authenticatedGroup.MapPost("/links", CreateLinkAsync)
            .WithName("CreateReferralLink")
            .Produces<CreateReferralLinkResponseDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

            // Method to list of referrals with its information
            authenticatedGroup.MapGet("", ListReferralsAsync)
           .WithName("ListMyReferrals")
           .Produces<ReferralListResponseDto>(StatusCodes.Status200OK)
           .ProducesProblem(StatusCodes.Status401Unauthorized);

            // Method to count the referrals by status
            authenticatedGroup.MapGet("/summary", GetSummaryAsync)
                .WithName("GetReferralSummary")
                .Produces<ReferralSummaryDto>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status401Unauthorized);


            #endregion

            #region Methods without authentication
            // Method to decide what experience the user should have when opening a referral link (referred vs non-referred)
            v1.MapPost("/referrals/resolve", ResolveAsync)
                .WithName("ResolveReferral")
                .Produces<ResolveReferralResponseDto>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status410Gone)
                .ProducesProblem(StatusCodes.Status429TooManyRequests);

            // hook to update the status of a referral, this one will be mostly used by the vendors
            v1.MapPost("/webhooks/deeplink/events", VendorEventAsync)
                .WithName("DeepLinkVendorEvent")
                .Produces(StatusCodes.Status202Accepted)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized)
                .ProducesProblem(StatusCodes.Status404NotFound);

            #endregion


            return app;
        }

        /// <summary>
        /// Method to manage the referral link creation
        /// </summary>
        private static async Task<IResult> CreateLinkAsync(
            HttpContext http,
            [FromBody] CreateReferralLinkRequestDto request,
            CreateReferralLinkUseCase createLinkUseCase,
            IIdempotencyStore idempotency,
            MockUserDirectory users,
            CancellationToken ct)
        {
            var userId = RequireUserId(http);

            // Idempotency handling (This is really helpfull when we're working wit mobile devices): if the header is present,
            // we will try to return the same response for the same request,
            // otherwise we will process it as a new one.
            // In real projects, we should consider the expiration of the idempotency keys and the storage cleanup strategy.
            var idemKey = http.Request.Headers["Idempotency-Key"].ToString();
            if (!string.IsNullOrWhiteSpace(idemKey))
            {
                var requestHash = RequestHasher.Sha256(request);

                var consistent = await idempotency.IsConsistentAsync(route: "/v1/referrals/links", key: idemKey, requestHash, ct);
                if (!consistent)
                    throw new AppException("idempotency_conflict", "Idempotency-Key was reused with a different request.");

                var cached = await idempotency.TryGetAsync("/v1/referrals/links", idemKey, ct);
                if (cached is not null)
                {
                    var dto = JsonSerializer.Deserialize<CreateReferralLinkResponseDto>(cached, ReferralsJsonContext.Default.CreateReferralLinkResponseDto);
                    return Results.Json(dto, statusCode: StatusCodes.Status201Created);
                }

                var currentCode = users.GetReferralCodeFor(userId);
                var response = await createLinkUseCase.ExecuteAsync(userId, currentCode, request, ct);

                var json = JsonSerializer.Serialize(response, ReferralsJsonContext.Default.CreateReferralLinkResponseDto);
                await idempotency.SaveAsync("/v1/referrals/links", idemKey, requestHash, json, ct);

                return Results.Json(response, statusCode: StatusCodes.Status201Created);
            }
            else
            {
                var currentCode = users.GetReferralCodeFor(userId);
                var response = await createLinkUseCase.ExecuteAsync(userId, currentCode, request, ct);
                return Results.Json(response, statusCode: StatusCodes.Status201Created);
            }
        }

        /// <summary>
        /// List My Referrals.
        /// </summary>
        private static async Task<IResult> ListReferralsAsync(
            HttpContext http,
            [FromQuery] string? status,
            [FromQuery] int? limit,
            [FromQuery] string? cursor,
            GetReferralsUseCase getReferralsUseCase,
            CancellationToken ct)
        {
            var userId = RequireUserId(http);
            var dto = await getReferralsUseCase.ExecuteAsync(userId, status, limit ?? 20, cursor, ct);
            return Results.Ok(dto);
        }

        /// <summary>
        /// Get Summary.
        /// </summary>
        private static async Task<IResult> GetSummaryAsync(
            HttpContext http,
            GetReferralSummaryUseCase getReferralSummayUseCase,
            CancellationToken ct)
        {
            var userId = RequireUserId(http);
            var dto = await getReferralSummayUseCase.ExecuteAsync(userId, ct);
            return Results.Ok(dto);
        }

        /// <summary>
        /// This method is the one who resolves the:
        /// Is this install coming from a referral? If yes, 
        /// which referral code should be applied and what onboarding should the app show
        /// </summary>
        private static async Task<IResult> ResolveAsync(
            [FromBody] ResolveReferralRequestDto request,
            ResolveReferralUseCase useCase,
            CancellationToken ct)
        {
            // Resolve returns 200 for known (referred) tokens and 404 for unknown tokens.
            var dto = await useCase.ExecuteAsync(request, ct);
            return Results.Ok(dto);
        }

        /// <summary>
        /// This would serve as a webhook for the vendors to notify us about certain events 
        /// related to the referrals, for example: app install, etc. 
        /// With this information we can update the status of the referrals and 
        /// trigger certain actions (send notifications, rewards, etc).
        /// </summary>
        private static async Task<IResult> VendorEventAsync(
            HttpContext http,
            [FromBody] VendorEventDto evt,
            HandleVendorEventUseCase handleVendorEventUseCase,
            CancellationToken ct)
        {
            var signature = http.Request.Headers[VendorSignatureHeader].ToString();
            if (!string.Equals(signature, VendorSignatureSecret, StringComparison.Ordinal))
                return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid signature.", extensions: new Dictionary<string, object?> { ["code"] = "invalid_signature" });

            await handleVendorEventUseCase.ExecuteAsync(evt, ct);
            return Results.Accepted(value: new AcceptedResponseDto(true));
        }


        /// <summary>
        /// Validates whether the request has the userId
        /// </summary>
        private static string RequireUserId(HttpContext http)
        {
            // Mock auth: required header for referrer endpoints.
            var userId = http.Request.Headers["X-Mock-UserId"].ToString();
            if (string.IsNullOrWhiteSpace(userId))
                throw new AppException("unauthorized", "Missing X-Mock-UserId header.");

            return userId;
        }
    }
}
