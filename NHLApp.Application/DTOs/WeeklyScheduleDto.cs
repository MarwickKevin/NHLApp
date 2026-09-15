using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NHLApp.Application.DTOs
{

    public record ScheduleRootDTO
    {
        [JsonPropertyName("gameWeek")]
        public List<GameWeekDTO>? GameWeek { get; set; }
    }

    public record GameWeekDTO
    {
        [JsonPropertyName("games")]
        public List<GameDTO>? Games { get; set; }
    }

    public record GameDTO
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
    }

}
