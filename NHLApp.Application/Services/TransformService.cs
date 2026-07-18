using NHLApp.Application.DTOs;
using NHLApp.Domain.Entities;
using NHLApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace NHLApp.Application.Services
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
            var raw = _db.RawApiResponses.FirstOrDefault(r => r.Endpoint == "season");
            if (raw == null) return;

            // Deserialize the JSON array of season IDs into a list of integers
            var seasonIds = JsonSerializer.Deserialize<List<int>>(raw.ResponseJson)!;

            // Idempotent insertion: only add seasons that don't already exist in the database
            foreach (var seasonId in seasonIds)
            {
                if (_db.Seasons.Any(s => s.SeasonId == seasonId)) continue;

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

            // Automatic deserialization using the DTO (ignoring JSON case)
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            NhlTeamRootDTO? root = JsonSerializer.Deserialize<NhlTeamRootDTO>(raw.ResponseJson, options);
            if (root?.Data == null) return;

            var addedFranchiseIds = new HashSet<int>();

            // Idempotent insertion: check for existing franchises and teams before adding
            foreach (NhlTeamItemDTO item in root.Data)
            {                
                if (item.FranchiseId.HasValue
                    && !addedFranchiseIds.Contains(item.FranchiseId.Value)
                    && !_db.Franchises.Any(f => f.FranchiseId == item.FranchiseId.Value))
                {
                    _db.Franchises.Add(new Franchise
                    {
                        FranchiseId = item.FranchiseId.Value,
                        Name = item.FullName
                    });
                    addedFranchiseIds.Add(item.FranchiseId.Value);
                }

                if (_db.Teams.Any(t => t.TeamId == item.Id)) continue;

                _db.Teams.Add(new Team
                {
                    TeamId = item.Id,
                    FranchiseId = item.FranchiseId,
                    FullName = item.FullName,
                    TriCode = item.TriCode,
                    RawTriCode = item.RawTricode,
                    LeagueId = item.LeagueId
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task TransformPlayersAsync()
        {
            // Fetch all raw roster JSON payloads from staging
            var rosterRaws = _db.RawApiResponses.Where(r => r.Endpoint == "roster").ToList();

            // Load existing tracking sets to avoid duplicates and round-trips
            var existingPlayerIds = _db.Players.Select(p => p.PlayerId).ToHashSet();
            var addedPlayerIds = new HashSet<int>();

            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            foreach (var raw in rosterRaws)
            {
                // Automatically deserialize the entire nested structure using the DTO
                NhlRosterRootDTO? rosterData = JsonSerializer.Deserialize<NhlRosterRootDTO>(raw.ResponseJson, jsonOptions);
                if (rosterData == null) continue;

                // Flatten the three positional lists into a single collection
                IEnumerable<NhlPlayerDTO> allPlayers = rosterData.Forwards
                    .Concat(rosterData.Defensemen)
                    .Concat(rosterData.Goalies);

                // Idempotent insertion: check for existing players before adding
                foreach (NhlPlayerDTO playerDto in allPlayers)
                {
                    if (existingPlayerIds.Contains(playerDto.Id) || addedPlayerIds.Contains(playerDto.Id))
                        continue;

                    _db.Players.Add(new Player
                    {
                        PlayerId = playerDto.Id,
                        FirstName = playerDto.FirstName.Default,
                        LastName = playerDto.LastName.Default,
                        Position = playerDto.PositionCode,
                        ShootsCatches = playerDto.ShootsCatches ?? string.Empty,
                        HeightInCentimeters = playerDto.HeightInCentimeters,
                        WeightInKilograms = playerDto.WeightInKilograms,
                        BirthDate = playerDto.BirthDate != null ? DateOnly.Parse(playerDto.BirthDate) : null,
                        BirthCity = playerDto.BirthCity?.Default,
                        BirthCountry = playerDto.BirthCountry
                    });

                    addedPlayerIds.Add(playerDto.Id);
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
