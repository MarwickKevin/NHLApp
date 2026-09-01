using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Domain.Entities
{
    public class Game
    {
        public int Id { get; set; }
        public int? Season { get; set; }
        public int? GameType { get; set; }
        public bool? LimitedScoring { get; set; }
        public string? GameDate { get; set; } 
        public string? VenueDefault { get; set; } 
        public string? VenueLocation { get; set; }
        public string? StartTimeUTC { get; set; } 
        public string? GameState { get; set; } 
        public string? GameScheduleState { get; set; } 
        public int? AwayTeamId { get; set; }
        public int? HomeTeamId { get; set; }
        public int? AwayScore { get; set; }
        public int? HomeScore { get; set; }
        public int? AwaySog { get; set; }
        public int? HomeSog { get; set; }
        public bool? ShootoutInUse { get; set; }
        public bool? OtInUse { get; set; }
        public string? LastPeriodType { get; set; } 
        public bool? Tie { get; set; }
        public int? OtPeriods { get; set; }
        public int? RegPeriods { get; set; }

        public int? PeriodNumber { get; set; }
        public string? PeriodType { get; set; }
        public int? MaxRegulationPeriods { get; set; }
        public int? PeriodDescriptorOtPeriods { get; set; }
        public string? ClockTimeRemaining { get; set; }
        public int? ClockSecondsRemaining { get; set; }
        public bool? ClockRunning { get; set; }
        public bool? ClockInIntermission { get; set; }
        public string? SpecialEventLightLogoUrl { get; set; }


        // Special Event Info (Outdoor games, All-Star, Stadium Series)
        public int? SpecialEventParentId { get; set; }
        public string? SpecialEventName { get; set; }
    }
}
