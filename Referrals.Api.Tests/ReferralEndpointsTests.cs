using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Referrals.Application.Dtos;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Referrals.Api.Tests
{
    /// <summary>
    /// Referral Endpoints Tests
    /// </summary>
    /// <remarks>
    /// Referral Endpoints Tests.
    /// </remarks>
    public sealed class ReferralEndpointsTests(WebApplicationFactory<Program> _factory) : IClassFixture<WebApplicationFactory<Program>>
    {

        [Fact]
        /// <summary>
        /// Create Link Without Auth Header Returns 401.
        /// </summary>
        public async Task CreateLink_WithoutAuthHeader_Returns401()
        {
            var client = _factory.CreateClient();

            var req = new CreateReferralLinkRequestDto(
                ReferralCode: "XY7G4D",
                Channel: "sms",
                Locale: "en-US",
                Campaign: "invite_friends",
                Destination: "signup",
                Client: new ClientInfoDto("ios", "7.12.0")
            );

            var resp = await client.PostAsJsonAsync("/v1/referrals/links", req);

            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
            problem!.Extensions["code"].ToString().Should().Be("unauthorized");
        }

        [Fact]
        /// <summary>
        /// Create Link With Idempotency Key Returns Same Response On Retry.
        /// </summary>
        public async Task CreateLink_WithIdempotencyKey_ReturnsSameResponseOnRetry()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Mock-UserId", "user_123");

            var req = new CreateReferralLinkRequestDto(
                ReferralCode: "XY7G4D",
                Channel: "sms",
                Locale: "en-US",
                Campaign: "invite_friends",
                Destination: "signup",
                Client: new ClientInfoDto("ios", "7.12.0")
            );

            var key = Guid.NewGuid().ToString();
            var msg1 = new HttpRequestMessage(HttpMethod.Post, "/v1/referrals/links")
            {
                Content = JsonContent.Create(req)
            };
            msg1.Headers.Add("Idempotency-Key", key);

            var resp1 = await client.SendAsync(msg1);
            resp1.StatusCode.Should().Be(HttpStatusCode.Created);
            var dto1 = await resp1.Content.ReadFromJsonAsync<CreateReferralLinkResponseDto>();
            dto1.Should().NotBeNull();
            dto1!.Url.Should().StartWith("https://cartoncaps.link/");
            dto1.Url.Should().Contain("?referral_code=XY7G4D");

            var msg2 = new HttpRequestMessage(HttpMethod.Post, "/v1/referrals/links")
            {
                Content = JsonContent.Create(req)
            };
            msg2.Headers.Add("Idempotency-Key", key);

            var resp2 = await client.SendAsync(msg2);
            resp2.StatusCode.Should().Be(HttpStatusCode.Created);
            var dto2 = await resp2.Content.ReadFromJsonAsync<CreateReferralLinkResponseDto>();

            dto2!.LinkId.Should().Be(dto1.LinkId);
            dto2.Token.Should().Be(dto1.Token);
            dto2.Url.Should().Be(dto1.Url);
        }

        [Fact]
        /// <summary>
        /// List Referrals User Has Seeded Complete Referrals.
        /// </summary>
        public async Task ListReferrals_UserHasSeededCompleteReferrals()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Mock-UserId", "user_123");

            var resp = await client.GetAsync("/v1/referrals?status=complete&limit=10");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await resp.Content.ReadFromJsonAsync<ReferralListResponseDto>();
            dto!.Items.Should().NotBeEmpty();
            dto.Items.All(i => i.Status == "complete").Should().BeTrue();
        }

        [Fact]
        /// <summary>
        /// Resolve Unknown Token Returns 404.
        /// </summary>
        public async Task Resolve_UnknownToken_Returns404()
        {
            var client = _factory.CreateClient();

            var req = new ResolveReferralRequestDto(
                Token: "dl_does_not_exist_12345678",
                Device: new DeviceInfoDto("device_hash", "ios", "7.12.0")
            );

            var resp = await client.PostAsJsonAsync("/v1/referrals/resolve", req);
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
            problem!.Extensions["code"].ToString().Should().Be("token_not_found");
        }

        [Fact]
        /// <summary>
        /// Vendor Webhook Invalid Signature Returns 401.
        /// </summary>
        public async Task VendorWebhook_InvalidSignature_Returns401()
        {
            var client = _factory.CreateClient();

            var evt = new VendorEventDto(
                EventType: "install",
                Token: "dl_seed_0_dummy",
                OccurredAt: DateTimeOffset.UtcNow,
                DeviceIdHash: "sha256:abc",
                Metadata: null
            );

            var msg = new HttpRequestMessage(HttpMethod.Post, "/v1/webhooks/deeplink/events")
            {
                Content = JsonContent.Create(evt)
            };
            msg.Headers.Add("X-Vendor-Signature", "wrong");

            var resp = await client.SendAsync(msg);
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
