using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using NHLApp.Application.Contexts;
using NHLApp.Application.Services;
using NHLApp.Domain.Interfaces;
using NHLApp.Infrastructure.Data;
using NHLApp.Infrastructure.NHL;
using NHLApp.Worker;
using Polly;
using Polly.Retry;
using System.Threading.RateLimiting;
using System.Xml.Serialization;


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<NHLAppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddHttpClient<INHLApiClient, NHLApiClient>()
    .AddResilienceHandler("nhl-pipeline", builder =>
    {
        builder.AddRateLimiter(new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 4,           // Maximum bursts allowed
            QueueLimit = 1000000,        // Max requests queued waiting for a token
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            TokensPerPeriod = 4,      // Number of tokens refilled per period (e.g., 3 requests/sec)
            AutoReplenishment = true
        }));

        builder.AddRetry(new HttpRetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .HandleResult(response =>
                    response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                    (int)response.StatusCode >= 500),
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(1),
            BackoffType = DelayBackoffType.Exponential
        });
    });

builder.Services.AddHostedService<Worker>();

builder.Services.AddScoped<ImportService>();

builder.Services.AddScoped<TransformService>();

builder.Services.AddScoped<WorkerContext>();

var host = builder.Build();

// Automatically apply migrations and create the DB if it doesn't exist (for easier testing purposes)
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NHLAppDbContext>();
    db.Database.Migrate();
}

host.Run();
