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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Franchise>()
                .HasKey(f => f.FranchiseId);

            modelBuilder.Entity<Franchise>()
                .Property(f => f.FranchiseId)
                .ValueGeneratedNever();

            modelBuilder.Entity<Team>()
                .HasKey(t => t.TeamId);

            modelBuilder.Entity<Team>()
                .Property(t => t.TeamId)
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
                .HasOne(t => t.Franchise)
                .WithMany(f => f.Teams)
                .HasForeignKey(t => t.FranchiseId);

            modelBuilder.Entity<RawApiResponse>()
                .Property(r => r.ResponseJson)
                .HasColumnType("nvarchar(max)");
        }

    }
}
