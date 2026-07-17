using Microsoft.EntityFrameworkCore;
using NHLApp.Domain.Interfaces;
using NHLApp.Worker;
using NHLApp.Application.Services;
using NHLApp.Infrastructure.Data;
using NHLApp.Infrastructure.NHL;

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
