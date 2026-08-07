using Microsoft.Extensions.Logging;
using NHLApp.Application.Contexts;
using NHLApp.Application.Extensions;
using NHLApp.Application.Services;
using NHLApp.Domain.Interfaces;
using NHLApp.Infrastructure.Data;

namespace NHLApp.Worker
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
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var importService = scope.ServiceProvider.GetRequiredService<ImportService>();
            var transformService = scope.ServiceProvider.GetRequiredService<TransformService>();
            var context = scope.ServiceProvider.GetRequiredService<WorkerContext>();

            // Log the start of the worker and record the start time
            _logger.LogInformationWithColor("Worker NHLApp démarré", ConsoleColor.Green);
            context.StartedAt = DateTime.Now;
            

            
            ////////////////////////////////////////////////////
            // Import data from the NHL API into the database //
            ////////////////////////////////////////////////////
            
            await importService.ImportSeasonsAsync(context);
            _logger.LogInformationWithColor("Import des saisons terminé", ConsoleColor.Green);

            await importService.ImportTeamsAsync(context);
            _logger.LogInformationWithColor("Import des équipes terminé", ConsoleColor.Green);

            await importService.ImportRosterSeasonsAsync(context);
            _logger.LogInformationWithColor("Import des saisons par équipe terminé", ConsoleColor.Green);

            await importService.ImportRostersAsync(context);
            _logger.LogInformationWithColor("Import des rosters terminé", ConsoleColor.Green);

            await importService.ImportPlayerLandingsAsync(context);
            _logger.LogInformationWithColor("Import des player landings terminé", ConsoleColor.Green);

            await importService.ImportWeeklySchedulesAsync(context);
            _logger.LogInformationWithColor("Import des cédule terminé", ConsoleColor.Green);

            await importService.ImportBoxScoresAsync(context); 
            _logger.LogInformationWithColor("Import des box scores terminé", ConsoleColor.Green);

            await importService.ImportPlayByPlayAsync(context);
            _logger.LogInformationWithColor("Import des play-by-play terminé", ConsoleColor.Green);

            //////////////////////////////////////////////////////////////////
            // Transform data from the database into the application models //
            //////////////////////////////////////////////////////////////////

            await transformService.TransformSeasonsAsync(context);
            _logger.LogInformationWithColor("Transformation des saisons terminée", ConsoleColor.Green);

            await transformService.TransformTeamsAsync(context);
            _logger.LogInformationWithColor("Transformation des équipes terminée", ConsoleColor.Green);

            await transformService.TransformPlayersAsync(context);
            _logger.LogInformationWithColor("Transformation des joueurs terminée", ConsoleColor.Green);

            await transformService.TransformRostersAsync(context);
            _logger.LogInformationWithColor("Transformation des rosters terminée", ConsoleColor.Green);

            await transformService.TransformPlayerLandingsAsync(context);
            _logger.LogInformationWithColor("Transformation des player landings terminée", ConsoleColor.Green);

            await transformService.TransformWeeklySchedulesAsync(context);
            _logger.LogInformationWithColor("Transformation cédules hebdomadaires en GameIds terminée", ConsoleColor.Green);


            // Log the end of the worker and record the end time
            context.FinishedAt = DateTime.Now;

            if (context.TotalImportErrors == 0 && context.TotalTransformErrors == 0)
            {
                _logger.LogInformationWithColor(
                    "Importations et Transformations se sont terminé avec succès à {FinishedAt}. Durée: {Duration}", 
                    ConsoleColor.Green, 
                    context.FinishedAt, 
                    context.Duration);
            }
            else
            {
                _logger.LogInformationWithColor(
                    "Importations et Transformations se sont terminé avec {TotalImportErrors} erreur(s) d'importation et {TotalTransformErrors} erreur(s) de transformation, à {FinishedAt}. Durée: {Duration}", 
                    ConsoleColor.Yellow, 
                    context.TotalImportErrors, 
                    context.TotalTransformErrors, 
                    context.FinishedAt, 
                    context.Duration);
            }
        }
    }
}

