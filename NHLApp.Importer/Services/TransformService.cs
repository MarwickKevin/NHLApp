using NHLApp.Core.Entities;
using NHLApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NHLApp.Importer.Services
{
    public class TransformService
    {
        private readonly NHLAppDbContext _db;

        public TransformService(NHLAppDbContext db)
        {
            _db = db;
        }

        public async Task TransformSeasonsAsync()
        {
            if (_db.Seasons.Any()) return;

            var raw = _db.RawApiResponses.FirstOrDefault(r => r.Endpoint == "season");
            if (raw == null) return;

            var seasonIds = JsonSerializer.Deserialize<List<int>>(raw.ResponseJson)!;

            foreach (var seasonId in seasonIds)
            {
                _db.Seasons.Add(new Season
                {
                    SeasonId = seasonId,
                    StartYear = seasonId / 10000,
                    EndYear = seasonId % 10000
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task TransformTeamsAsync()
        {
            var raw = _db.RawApiResponses.FirstOrDefault(r => r.Endpoint == "team");
            if (raw == null) return;

            if (_db.Teams.Any()) return;

            var root = JsonDocument.Parse(raw.ResponseJson).RootElement;
            var data = root.GetProperty("data");

            var addedFranchiseIds = new HashSet<int>();

            foreach (var item in data.EnumerateArray())
            {
                var teamId = item.GetProperty("id").GetInt32();

                int? franchiseId = item.GetProperty("franchiseId").ValueKind == JsonValueKind.Null
                    ? null
                    : item.GetProperty("franchiseId").GetInt32();

                if (franchiseId.HasValue
                    && !addedFranchiseIds.Contains(franchiseId.Value)
                    && !_db.Franchises.Any(f => f.FranchiseId == franchiseId.Value))
                {
                    _db.Franchises.Add(new Franchise
                    {
                        FranchiseId = franchiseId.Value,
                        Name = item.GetProperty("fullName").GetString() ?? string.Empty
                    });
                    addedFranchiseIds.Add(franchiseId.Value);
                }

                _db.Teams.Add(new Team
                {
                    TeamId = teamId,
                    FranchiseId = franchiseId,
                    FullName = item.GetProperty("fullName").GetString() ?? string.Empty,
                    TriCode = item.GetProperty("triCode").GetString() ?? string.Empty,
                    RawTriCode = item.GetProperty("rawTricode").GetString() ?? string.Empty,
                    LeagueId = item.GetProperty("leagueId").GetInt32()
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task TransformPlayersAsync()
        {
            var rosterRaws = _db.RawApiResponses
                .Where(r => r.Endpoint == "roster")
                .ToList();

            var existingPlayerIds = _db.Players
                .Select(p => p.PlayerId)
                .ToHashSet();

            var addedPlayerIds = new HashSet<int>();

            foreach (var raw in rosterRaws)
            {
                var root = JsonDocument.Parse(raw.ResponseJson).RootElement;
                var sections = new[] { "forwards", "defensemen", "goalies" };
                foreach (var section in sections)
                {
                    if (!root.TryGetProperty(section, out var players))
                        continue;
                    foreach (var item in players.EnumerateArray())
                    {
                        var playerId = item.GetProperty("id").GetInt32();
                        if (existingPlayerIds.Contains(playerId) || addedPlayerIds.Contains(playerId))
                            continue;
                        _db.Players.Add(new Player
                        {
                            PlayerId = playerId,
                            FirstName = item.GetProperty("firstName").GetProperty("default").GetString() ?? string.Empty,
                            LastName = item.GetProperty("lastName").GetProperty("default").GetString() ?? string.Empty,
                            Position = item.GetProperty("positionCode").GetString() ?? string.Empty,
                            ShootsCatches = item.TryGetProperty("shootsCatches", out var sc) ? sc.GetString() ?? string.Empty : string.Empty,
                            HeightInCentimeters = item.TryGetProperty("heightInCentimeters", out var h) ? h.GetInt32() : null,
                            WeightInKilograms = item.TryGetProperty("weightInKilograms", out var w) ? w.GetInt32() : null,
                            BirthDate = item.TryGetProperty("birthDate", out var bd) ? DateOnly.Parse(bd.GetString()!) : null,
                            BirthCity = item.TryGetProperty("birthCity", out var bc) ? bc.GetProperty("default").GetString() : null,
                            BirthCountry = item.TryGetProperty("birthCountry", out var bco) ? bco.GetString() : null,
                        });
                        addedPlayerIds.Add(playerId);
                    }
                }
            }
            await _db.SaveChangesAsync();
        }
    }
}
