using NHLApp.Application.Contexts;
using NHLApp.Application.Services;
using NHLApp.Infrastructure.Data;

namespace NHLApp.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        public WorkerContext _context { get; private set; }

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, WorkerContext context)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker NHLApp démarré");

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NHLAppDbContext>();
            var importService = scope.ServiceProvider.GetRequiredService<ImportService>();
            var transformService = scope.ServiceProvider.GetRequiredService<TransformService>();


            // Import data from the NHL API into the database
            
            await importService.ImportSeasonsAsync();
            _logger.LogInformation("Import des saisons terminé");

            await importService.ImportTeamsAsync();
            _logger.LogInformation("Import des équipes terminé");

            await importService.ImportRosterSeasonsAsync();
            _logger.LogInformation("Import des saisons par équipe terminé");

            await importService.ImportRostersAsync();
            _logger.LogInformation("Import des rosters terminé");


            // Transform data from the database into the application models

            await transformService.TransformSeasonsAsync();
            _logger.LogInformation("Transformation des saisons terminée");

            await transformService.TransformTeamsAsync();
            _logger.LogInformation("Transformation des équipes terminée");    

            await transformService.TransformPlayersAsync();
            _logger.LogInformation("Transformation des joueurs terminée");

            await transformService.TransformRostersAsync();
            _logger.LogInformation("Transformation des rosters terminée");

        }
    }
}
