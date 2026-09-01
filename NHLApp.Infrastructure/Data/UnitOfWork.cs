using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using NHLApp.Domain.Entities;
using NHLApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly NHLAppDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;

        public UnitOfWork(NHLAppDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context;
            _logger = logger;
        }

        public DbSet<RawApiResponse> RawApiResponses => _context.RawApiResponses;
        public DbSet<Season> Seasons => _context.Seasons;
        public DbSet<Franchise> Franchises => _context.Franchises;
        public DbSet<Team> Teams => _context.Teams;
        public DbSet<Player> Players => _context.Players;
        public DbSet<TeamRosters> TeamRosters => _context.TeamRosters;
        public DbSet<Trophy> Trophies => _context.Trophies;
        public DbSet<DraftDetail> DraftDetail => _context.DraftDetail;
        public DbSet<PlayerAwards> PlayerAwards => _context.PlayerAwards;
        public DbSet<SeasonTotal> SeasonTotals => _context.SeasonTotals;
        public DbSet<Game> Games => _context.Games; 
        public DbSet<GamePlay> GamePlays => _context.GamePlays;
        public DbSet<PlayerGameStat> PlayerGameStats => _context.PlayerGameStats;
        public DbSet<GoalieGameStat> GoalieGameStats => _context.GoalieGameStats;

        public ChangeTracker ChangeTracker => _context.ChangeTracker;

        public Microsoft.EntityFrameworkCore.Metadata.IModel Model => _context.Model;

        public DbSet<T> Set<T>() where T : class
        {
            return _context.Set<T>();
        }


        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);

        public async Task SaveOrUpdateRawResponseAsync(string endpoint, string entityId, string json, string? metadata = null)
        {
            try
            {
                var existing = await _context.RawApiResponses
                    .FirstOrDefaultAsync(r => r.Endpoint == endpoint && r.EntityId == entityId);

                if (existing != null)
                {
                    existing.ResponseJson = json;
                    existing.Metadata = metadata; // Mise à jour
                    existing.FetchedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.RawApiResponses.Add(new RawApiResponse
                    {
                        Endpoint = endpoint,
                        EntityId = entityId,
                        ResponseJson = json,
                        Metadata = metadata,
                        FetchedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Failed to save endpoint {Endpoint} for entity {EntityId}.", endpoint, entityId);
            }
        }
    }
}
