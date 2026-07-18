using NHLApp.Domain.Entities;
using NHLApp.Domain.Interfaces;
using NHLApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NHLApp.Domain;


namespace NHLApp.Application.Services
{
    public class ImportService
    {
        private readonly INHLApiClient _nhlClient;
        private readonly NHLAppDbContext _db;

        public ImportService(INHLApiClient nhlClient, NHLAppDbContext db)
        {
            _nhlClient = nhlClient;
            _db = db;
        }

       
        public async Task ImportSeasonsAsync()
        {
            var existing = _db.RawApiResponses
                .FirstOrDefault(r => r.Endpoint == "season");

            // TODO: Change this to a more appropriate cache duration
            if (existing != null && existing.FetchedAt > DateTime.UtcNow.AddYears(-1))
                return;

            /// Retrieve the seasons from the NHL API
            var json = await _nhlClient.GetSeasonsAsync();

            if (existing != null)
            {
                existing.ResponseJson = json;
                existing.FetchedAt = DateTime.UtcNow;
            }
            else
            {
                _db.RawApiResponses.Add(new RawApiResponse
                {
                    Endpoint = "season",
                    EntityId = "all",
                    ResponseJson = json,
                    FetchedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task ImportTeamsAsync()
        {
            if (_db.RawApiResponses.Any(r => r.Endpoint == "team"))
                return;

            var json = await _nhlClient.GetTeamsAsync();

            _db.RawApiResponses.Add(new RawApiResponse
            {
                Endpoint = "team",
                EntityId = "all",
                ResponseJson = json,
                FetchedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
        }

        public async Task ImportRosterSeasonsAsync()
        {
            var teams = _db.Teams
                .Where(t => t.TriCode != "TBD" && t.TriCode != "NHL")
                .ToList();

            foreach (var team in teams)
            {
                var key = $"roster-seasons-{team.TriCode}";

                if (_db.RawApiResponses.Any(r => r.EntityId == key))
                    continue;

                try
                {
                    var json = await _nhlClient.GetTeamRosterSeasonsAsync(team.TriCode);

                    _db.RawApiResponses.Add(new RawApiResponse
                    {
                        Endpoint = "roster-seasons",
                        EntityId = key,
                        ResponseJson = json,
                        FetchedAt = DateTime.UtcNow
                    });

                    await _db.SaveChangesAsync();
                    await Task.Delay(300);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur roster-seasons {team.TriCode}: {ex.Message}");
                }
            }
        }

        public async Task ImportRostersAsync()
        {
            var teams = _db.Teams
                .Where(t => t.TriCode != "TBD" && t.TriCode != "NHL")
                .ToList();

            foreach (var team in teams)
            {
                var rosterSeasonsRaw = _db.RawApiResponses
                    .FirstOrDefault(r => r.EntityId == $"roster-seasons-{team.TriCode}");

                if (rosterSeasonsRaw == null) continue;

                var seasonIds = JsonSerializer.Deserialize<List<int>>(rosterSeasonsRaw.ResponseJson)!;

                foreach (var seasonId in seasonIds)
                {
                    var key = $"roster-{team.TriCode}-{seasonId}";

                    if (_db.RawApiResponses.Any(r => r.EntityId == key))
                        continue;

                    try
                    {
                        var json = await _nhlClient.GetTeamRosterAsync(team.TriCode, seasonId);

                        _db.RawApiResponses.Add(new RawApiResponse
                        {
                            Endpoint = "roster",
                            EntityId = key,
                            ResponseJson = json,
                            FetchedAt = DateTime.UtcNow
                        });

                        await _db.SaveChangesAsync();
                        //await Task.Delay(10);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur roster {team.TriCode} {seasonId}: {ex.Message}");
                    }
                }
            }
        }

    }
}
