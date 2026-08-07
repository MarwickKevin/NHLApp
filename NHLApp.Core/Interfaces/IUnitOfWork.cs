using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NHLApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        DbSet<RawApiResponse> RawApiResponses { get; }
        DbSet<Season> Seasons { get; }
        DbSet<Franchise> Franchises { get; }
        DbSet<Team> Teams { get; }
        DbSet<Player> Players { get; }
        DbSet<TeamRosters> TeamRosters { get; }
        DbSet<Trophy> Trophies { get; }
        DbSet<DraftDetail> DraftDetail { get; }
        DbSet<PlayerAwards> PlayerAwards { get; }
        DbSet<SeasonTotal> SeasonTotals { get; }

        ChangeTracker ChangeTracker { get; }

        Microsoft.EntityFrameworkCore.Metadata.IModel Model { get; }

        DbSet<T> Set<T>() where T : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task SaveOrUpdateRawResponseAsync(string endpoint, string entityId, string json);
    }
}
