using Microsoft.EntityFrameworkCore;
using NHLApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Infrastructure.Data
{
    public class NHLAppDbContext : DbContext
    {
        public NHLAppDbContext(DbContextOptions<NHLAppDbContext> options) : base(options)
        {
        }

        public DbSet<Season> Seasons { get; set; }
        public DbSet<Franchise> Franchises { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<RawApiResponse> RawApiResponses { get; set; }
        public DbSet<TeamRosters> TeamRosters { get; set; }
        public DbSet<Trophy> Trophies { get; set; }
        public DbSet<DraftDetail> DraftDetail { get; set; }
        public DbSet<PlayerAwards> PlayerAwards { get; set; }
        public DbSet<SeasonTotal> SeasonTotals { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GamePlay> GamePlays { get; set; }
        public DbSet<PlayerGameStat> PlayerGameStats { get; set; }
        public DbSet<GoalieGameStat> GoalieGameStats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Raw API Responses
            modelBuilder.Entity<RawApiResponse>()
                .Property(r => r.ResponseJson)
                .HasColumnType("nvarchar(max)");

            // Franchises
            modelBuilder.Entity<Franchise>(entity =>
            {
                entity.HasKey(f => f.FranchiseId);
                entity.Property(f => f.FranchiseId).ValueGeneratedNever();
            });

            // Players
            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(p => p.PlayerId);
                entity.Property(p => p.PlayerId).ValueGeneratedNever();
            });


            // Seasons
            modelBuilder.Entity<Season>(entity =>
            {
                entity.HasKey(s => s.SeasonId);
                entity.Property(s => s.SeasonId).ValueGeneratedNever();
            });

            // Trophies
            modelBuilder.Entity<Trophy>(entity =>
            {
                entity.HasKey(t => t.Id);
            });

            // Teams
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(t => new { t.TeamId, t.SeasonId });
                entity.Property(t => t.TeamId).ValueGeneratedNever();

                entity.HasOne(t => t.Franchise)
                      .WithMany(f => f.Teams)
                      .HasForeignKey(t => t.FranchiseId);

                entity.HasOne(t => t.Season)
                      .WithMany()
                      .HasForeignKey(t => t.SeasonId);
            });

            // TeamRosters
            modelBuilder.Entity<TeamRosters>(entity =>
            {
                entity.HasKey(tr => new { tr.TeamId, tr.PlayerId, tr.SeasonId });

                entity.HasOne(tr => tr.Team)
                      .WithMany()
                      .HasForeignKey(tr => new { tr.TeamId, tr.SeasonId });

                entity.HasOne(tr => tr.Player)
                      .WithMany()
                      .HasForeignKey(tr => tr.PlayerId);
            });

            // DraftDetail
            modelBuilder.Entity<DraftDetail>(entity =>
            {
                entity.HasKey(d => d.PlayerId);
            });

            // PlayerAwards
            modelBuilder.Entity<PlayerAwards>(entity =>
            {
                entity.HasKey(pa => new { pa.PlayerId, pa.TrophyId, pa.SeasonId });

                entity.HasOne(pa => pa.Player)
                      .WithMany(p => p.PlayerAwards)
                      .HasForeignKey(pa => pa.PlayerId);

                entity.HasOne(pa => pa.Trophy)
                      .WithMany(t => t.Awards)
                      .HasForeignKey(pa => pa.TrophyId);

                entity.HasOne(pa => pa.Season)
                      .WithMany(s => s.Awards)
                      .HasForeignKey(pa => pa.SeasonId);
            });

            // Games
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Id).ValueGeneratedNever();
                entity.Property(g => g.GameDate).HasMaxLength(20);
                entity.Property(g => g.VenueDefault).HasMaxLength(150);
                entity.Property(g => g.VenueLocation).HasMaxLength(100);
                entity.Property(g => g.StartTimeUTC).HasMaxLength(35);
                entity.Property(g => g.GameState).HasMaxLength(10);
                entity.Property(g => g.GameScheduleState).HasMaxLength(10);
                entity.Property(g => g.LastPeriodType).HasMaxLength(20);
                entity.Property(g => g.SpecialEventName).HasMaxLength(150);

                entity.HasOne<Season>()
                      .WithMany()
                      .HasForeignKey(g => g.SeasonId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // GamePlays
            modelBuilder.Entity<GamePlay>(entity =>
            {
                entity.HasKey(gp => gp.Id);
                entity.Property(gp => gp.PeriodType).HasMaxLength(20);
                entity.Property(gp => gp.TimeInPeriod).HasMaxLength(10);
                entity.Property(gp => gp.TimeRemaining).HasMaxLength(10);
                entity.Property(gp => gp.TypeDescKey).HasMaxLength(50);
                entity.Property(gp => gp.SituationCode).HasMaxLength(10);
                entity.Property(gp => gp.HomeTeamDefendingSide).HasMaxLength(5);

                entity.Property(gp => gp.ShotType).HasMaxLength(50);
                entity.Property(gp => gp.Reason).HasMaxLength(100);
                entity.Property(gp => gp.SecondaryReason).HasMaxLength(100);
                entity.Property(gp => gp.ZoneCode).HasMaxLength(5);

                entity.HasOne<Game>()
                      .WithMany()
                      .HasForeignKey(gp => gp.GameId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(gp => new { gp.GameId, gp.EventId }).IsUnique();
            });

            // PlayerGameStats
            modelBuilder.Entity<PlayerGameStat>(entity =>
            {
                entity.HasKey(pgs => pgs.Id);
                entity.Property(pgs => pgs.Position).HasMaxLength(5);
                entity.Property(pgs => pgs.Toi).HasMaxLength(10);

                entity.HasOne<Game>()
                      .WithMany()
                      .HasForeignKey(pgs => pgs.GameId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Player>()
                      .WithMany()
                      .HasForeignKey(pgs => pgs.PlayerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(pgs => new { pgs.GameId, pgs.PlayerId, pgs.TeamId }).IsUnique();
            });

            // GoalieGameStats
            modelBuilder.Entity<GoalieGameStat>(entity =>
            {
                entity.HasKey(ggs => ggs.Id);
                entity.Property(ggs => ggs.Toi).HasMaxLength(10);
                entity.Property(ggs => ggs.Decision).HasMaxLength(5);
                entity.Property(ggs => ggs.EvenStrengthShotsAgainst).HasMaxLength(15);
                entity.Property(ggs => ggs.PowerPlayShotsAgainst).HasMaxLength(15);
                entity.Property(ggs => ggs.ShorthandedShotsAgainst).HasMaxLength(15);
                entity.Property(ggs => ggs.SaveShotsAgainst).HasMaxLength(15);

                entity.HasOne<Game>()
                      .WithMany()
                      .HasForeignKey(ggs => ggs.GameId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Player>()
                      .WithMany()
                      .HasForeignKey(ggs => ggs.PlayerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(ggs => new { ggs.GameId, ggs.PlayerId, ggs.TeamId }).IsUnique();
            });
        }
    }
}
