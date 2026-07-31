using Microsoft.EntityFrameworkCore;
using NHLApp.Domain.Interfaces;
using NHLApp.Worker;
using NHLApp.Application.Services;
using NHLApp.Infrastructure.Data;
using NHLApp.Infrastructure.NHL;
using System.Xml.Serialization;
using NHLApp.Application.Contexts;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<NHLAppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddHttpClient<INHLApiClient, NHLApiClient>();

builder.Services.AddHostedService<Worker>();

builder.Services.AddScoped<ImportService>();

builder.Services.AddScoped<TransformService>();

builder.Services.AddScoped<RawDataStore>();

builder.Services.AddScoped<WorkerContext>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 1. Définir le niveau global par défaut à Warning (masque HttpClient, EF Core, etc.)
builder.Logging.SetMinimumLevel(LogLevel.Warning);

// 2. Réactiver explicitement le niveau Information uniquement pour votre application
builder.Logging.AddFilter("NHLApp", LogLevel.Information);

var host = builder.Build();

// Automatically apply migrations and create the DB if it doesn't exist
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NHLAppDbContext>();
    db.Database.Migrate();
}

host.Run();
