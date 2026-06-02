using Microsoft.EntityFrameworkCore;
using NHLApp.Core.Interfaces;
using NHLApp.Importer;
using NHLApp.Importer.NHL;
using NHLApp.Importer.Services;
using NHLApp.Infrastructure.Data;

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

var host = builder.Build();
host.Run();
