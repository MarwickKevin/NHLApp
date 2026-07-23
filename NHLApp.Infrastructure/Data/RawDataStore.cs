using Microsoft.Extensions.Logging;
using NHLApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Infrastructure.Data
{
    public class RawDataStore
    {
        private readonly NHLAppDbContext _db;
        private readonly ILogger<RawDataStore> _logger;

        public RawDataStore(NHLAppDbContext db, ILogger<RawDataStore> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task SaveOrUpdateAsync(string endpoint, string entityId, string json, bool trackChanges = false)
        {
            try
            {
                var dbRecord = trackChanges
                  ? (_db.RawApiResponses.Local.FirstOrDefault(r => r.EntityId == entityId) ?? _db.RawApiResponses.FirstOrDefault(r => r.EntityId == entityId))
                  : _db.RawApiResponses.FirstOrDefault(r => r.EntityId == entityId);

                if (dbRecord != null)
                {
                    if (dbRecord.ResponseJson != json)
                    {
                        dbRecord.ResponseJson = json;
                        dbRecord.FetchedAt = DateTime.UtcNow;
                        await _db.SaveChangesAsync();
                    }
                }
                else
                {
                    _db.RawApiResponses.Add(new RawApiResponse
                    {
                        Endpoint = endpoint,
                        EntityId = entityId,
                        ResponseJson = json,
                        FetchedAt = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save endpoint {Endpoint} for entity {EntityId}.", endpoint, entityId);
            }
        }
    }
}