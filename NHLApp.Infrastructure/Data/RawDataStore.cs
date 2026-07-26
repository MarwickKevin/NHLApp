using Microsoft.EntityFrameworkCore;
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

        public async Task SaveOrUpdateAsync(string endpoint, string entityId, string json)
        {
            try
            {
                // Check if the record already exists in the database, considering whether to track changes or not
                var dbRecord = await _db.RawApiResponses.FirstOrDefaultAsync(r => r.EntityId == entityId);

                // If the record exists, update it; otherwise, create a new record
                if (dbRecord != null)
                {                    
                        dbRecord.ResponseJson = json;
                        dbRecord.FetchedAt = DateTime.UtcNow;

                        await _db.SaveChangesAsync();                    
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