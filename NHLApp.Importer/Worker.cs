using NHLApp.Importer.Services;
using NHLApp.Infrastructure.Data;

namespace NHLApp.Importer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Importer démarré");

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NHLAppDbContext>();
            var importService = scope.ServiceProvider.GetRequiredService<ImportService>();
            var transformService = scope.ServiceProvider.GetRequiredService<TransformService>();

            await importService.ImportSeasonsAsync();
            _logger.LogInformation("Import des saisons terminé");

            await transformService.TransformSeasonsAsync();
            _logger.LogInformation("Transformation des saisons terminée");

            await importService.ImportTeamsAsync();
            _logger.LogInformation("Import des équipes terminé");

            await transformService.TransformTeamsAsync();
            _logger.LogInformation("Transformation des équipes terminée");

            await importService.ImportRosterSeasonsAsync();
            _logger.LogInformation("Import des saisons par équipe terminé");
            
            await importService.ImportRostersAsync();
            _logger.LogInformation("Import des rosters terminé");

            await transformService.TransformPlayersAsync();
            _logger.LogInformation("Transformation des joueurs terminée");


        }
    }
}
