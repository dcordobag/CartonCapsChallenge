using Microsoft.Extensions.DependencyInjection;
using Referrals.Application.Abstractions;
using Referrals.Application.UseCases.CreateReferralLink;
using Referrals.Application.UseCases.GetReferrals;
using Referrals.Application.UseCases.GetReferralSummary;
using Referrals.Application.UseCases.HandleVendorEvent;
using Referrals.Application.UseCases.ResolveReferral;
using Referrals.Infrastructure.Persistence;
using Referrals.Infrastructure.Profiles;
using Referrals.Infrastructure.Security;
using Referrals.Infrastructure.Seed;
using Referrals.Infrastructure.Vendors;

namespace Referrals.Infrastructure
{

    /// <summary>
    /// Dependency Injection
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Add Referral Infrastructure.
        /// </summary>
        public static IServiceCollection AddReferralInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<InMemoryStore>();


            services.AddSingleton<IReferralLinkRepository, InMemoryReferralLinkRepository>();
            services.AddSingleton<IReferralRepository, InMemoryReferralRepository>();

            services.AddSingleton<IRateLimiter>(_ => new InMemoryRateLimiter(_maxEventsPerWindow: 5));
            services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

            services.AddSingleton<IDeepLinkVendorClient, FakeDeepLinkVendorClient>();
            services.AddSingleton<IReferrerInfoProvider, FakeReferrerInfoProvider>();

            services.AddSingleton<ReferralSeeder>();

            // Use cases
            services.AddScoped<CreateReferralLinkUseCase>();
            services.AddScoped<GetReferralsUseCase>();
            services.AddScoped<GetReferralSummaryUseCase>();
            services.AddScoped<ResolveReferralUseCase>();
            services.AddScoped<HandleVendorEventUseCase>();

            return services;
        }
    }
}
