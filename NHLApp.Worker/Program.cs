using Microsoft.EntityFrameworkCore;
using NHLApp.Application.Contexts;
using NHLApp.Application.Services;
using NHLApp.Domain.Interfaces;
using NHLApp.Infrastructure.Data;
using NHLApp.Infrastructure.NHL;
using NHLApp.Worker;
using Polly;
using Polly.Retry;
using System.Xml.Serialization;
using Microsoft.Extensions.Http.Resilience;


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
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .HandleResult(response => !response.IsSuccessStatusCode),
            MaxRetryAttempts = 9,
            Delay = TimeSpan.FromSeconds(0.2),
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
