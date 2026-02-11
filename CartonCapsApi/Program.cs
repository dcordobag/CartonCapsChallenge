using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Referrals.Api.Endpoints;
using Referrals.Api.Mocks;
using Referrals.Api.Serialization;
using Referrals.Application.Common;
using Referrals.Domain.Exceptions;
using Referrals.Infrastructure;
using Referrals.Infrastructure.Seed;


var builder = WebApplication.CreateSlimBuilder(args);

// Register the regex route constraint which is not added by default when using CreateSlimBuilder
builder.Services.Configure<RouteOptions>(options =>
{
    options.SetParameterPolicy<Microsoft.AspNetCore.Routing.Constraints.RegexInlineRouteConstraint>("regex");
});
builder.Services.ConfigureHttpJsonOptions(o =>
{
    // Make our DTO metadata available to Minimal APIs + OpenAPI schema generation
    o.SerializerOptions.TypeInfoResolverChain.Insert(0, ReferralsJsonContext.Default);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//CUSTOM SERVICES REGISTRATION
builder.Services.AddReferralInfrastructure();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new() { Title = "Carton Caps Referrals Mock API", Version = "v1" });
});

builder.Services.AddSingleton<MockUserDirectory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
        var ex = feature?.Error;
        if (ex is null) return;

        var (status, code, title) = ex switch
        {
            AppException a => (MapStatus(a.Code), a.Code, a.Message),
            DomainException d => (400, d.Code, d.Message),
            _ => (500, "internal_error", "Unexpected error.")
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = $"https://cartoncaps.link/problems/{code}",
            Detail = ex is AppException or DomainException ? ex.Message : null,
            Instance = feature?.Path
        };

        problem.Extensions["code"] = code;

        // Use Results.Problem which will produce a ProblemDetails body honoring the app's JsonOptions
        var extensions = new Dictionary<string, object?> { ["code"] = code };
        await Results.Problem(detail: problem.Detail, statusCode: problem.Status, title: problem.Title, type: problem.Type, extensions: extensions).ExecuteAsync(context);
    });
});

app.UseSwagger();
app.UseSwaggerUI();

// Seed data (in-memory)
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<ReferralSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

app.MapGet("/health", () => Results.Ok(new Referrals.Application.Dtos.HealthResponseDto("ok")))
    .WithName("Health");


// API endpoints
app.MapReferralEndpoints();

app.Run();

/// <summary>
/// Map Status.
/// </summary>
static int MapStatus(string code) => code switch
{
    "unauthorized" => StatusCodes.Status401Unauthorized,
    "referral_code_mismatch" => StatusCodes.Status403Forbidden,
    "token_not_found" => StatusCodes.Status404NotFound,
    "token_expired" => StatusCodes.Status410Gone,
    "rate_limited" => StatusCodes.Status429TooManyRequests,
    "invalid_request" => StatusCodes.Status400BadRequest,
    "vendor_unavailable" => StatusCodes.Status503ServiceUnavailable,
    "idempotency_conflict" => StatusCodes.Status409Conflict,
    _ => StatusCodes.Status400BadRequest
};