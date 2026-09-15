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

            modelBuilder.Entity<RawApiResponse>()
                .Property(r => r.ResponseJson)
                .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<Franchise>()
                .HasKey(f => f.FranchiseId);

            modelBuilder.Entity<Franchise>()
                .Property(f => f.FranchiseId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Player>()
                .HasKey(p => p.PlayerId);

            modelBuilder.Entity<Player>()
                .Property(p => p.PlayerId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Season>()
                .HasKey(s => s.SeasonId);

            modelBuilder.Entity<Season>()
                .Property(s => s.SeasonId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Team>()
                .HasKey(t => new { t.TeamId, t.SeasonId });

            modelBuilder.Entity<Team>()
                .Property(t => t.TeamId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Franchise)
                .WithMany(f => f.Teams)
                .HasForeignKey(t => t.FranchiseId);

            modelBuilder.Entity<Team>()
                .HasOne(t => t.Season)
                .WithMany()
                .HasForeignKey(t => t.SeasonId);

            modelBuilder.Entity<TeamRosters>()
                .HasKey(tr => new { tr.TeamId, tr.PlayerId, tr.SeasonId });

            modelBuilder.Entity<TeamRosters>()
                .HasOne(tr => tr.Team)
                .WithMany()
                .HasForeignKey(tr => new { tr.TeamId, tr.SeasonId });

            modelBuilder.Entity<TeamRosters>()
                .HasOne(tr => tr.Player)
                .WithMany()
                .HasForeignKey(tr => tr.PlayerId);

            modelBuilder.Entity<DraftDetail>()
                .HasKey(d => d.PlayerId);
            
            modelBuilder.Entity<PlayerAwards>()
                .HasKey(pa => new { pa.PlayerId, pa.TrophyId, pa.SeasonId });
            
            modelBuilder.Entity<PlayerAwards>()
                .HasOne(pa => pa.Player)
                .WithMany(p => p.PlayerAwards)
                .HasForeignKey(pa => pa.PlayerId);

            modelBuilder.Entity<PlayerAwards>()
                .HasOne(pa => pa.Trophy)
                .WithMany(t => t.Awards)
                .HasForeignKey(pa => pa.TrophyId);

            modelBuilder.Entity<PlayerAwards>()
                .HasOne(pa => pa.Season)
                .WithMany(s => s.Awards)
                .HasForeignKey(pa => pa.SeasonId);

            modelBuilder.Entity<Game>()
                .Property(g => g.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Game>(entity =>
            {
                entity.Property(g => g.GameDate).HasMaxLength(20);
                entity.Property(g => g.VenueDefault).HasMaxLength(150);
                entity.Property(g => g.VenueLocation).HasMaxLength(100);
                entity.Property(g => g.StartTimeUTC).HasMaxLength(35);
                entity.Property(g => g.GameState).HasMaxLength(10);
                entity.Property(g => g.GameScheduleState).HasMaxLength(10);
                entity.Property(g => g.LastPeriodType).HasMaxLength(20);
                entity.Property(g => g.SpecialEventName).HasMaxLength(150);
            });

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

            // PlayerGameStat Configuration
            modelBuilder.Entity<PlayerGameStat>(entity =>
            {
                entity.HasKey(pgs => pgs.Id);
                entity.Property(pgs => pgs.Position).HasMaxLength(5);
                entity.Property(pgs => pgs.Toi).HasMaxLength(10);

                entity.HasOne<Game>()
                      .WithMany()
                      .HasForeignKey(pgs => pgs.GameId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(pgs => new { pgs.GameId, pgs.PlayerId, pgs.TeamId }).IsUnique();
            });

            // GoalieGameStat Configuration
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

                entity.HasIndex(ggs => new { ggs.GameId, ggs.PlayerId, ggs.TeamId }).IsUnique();
            });


        }
    }
}
