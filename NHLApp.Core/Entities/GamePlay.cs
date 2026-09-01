using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Domain.Entities
{
    public class GamePlay
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int EventId { get; set; }
        public int PeriodNumber { get; set; }
        public string? PeriodType { get; set; }
        public string? TimeInPeriod { get; set; }
        public string? TimeRemaining { get; set; }
        public int TypeCode { get; set; }
        public string? TypeDescKey { get; set; }
        public int SortOrder { get; set; }
        public string? SituationCode { get; set; }
        public string? HomeTeamDefendingSide { get; set; }

        // Event Ownership & Scores
        public int? EventOwnerTeamId { get; set; }
        public int? AwayScore { get; set; }
        public int? HomeScore { get; set; }
        public int? AwaySOG { get; set; }
        public int? HomeSOG { get; set; }

        // Shot & Goal Details
        public string? ShotType { get; set; }
        public int? ShootingPlayerId { get; set; }
        public int? ScoringPlayerId { get; set; }
        public int? ScoringPlayerTotal { get; set; }
        public int? Assist1PlayerId { get; set; }
        public int? Assist1PlayerTotal { get; set; }
        public int? Assist2PlayerId { get; set; }
        public int? Assist2PlayerTotal { get; set; }
        public int? Assist3PlayerId { get; set; }
        public int? Assist3PlayerTotal { get; set; }
        public int? GoalieInNetId { get; set; }
        public int? GoalInGame { get; set; }

        // Hits, Blocks, & Faceoffs
        public int? HittingPlayerId { get; set; }
        public int? HitteePlayerId { get; set; }
        public int? BlockingPlayerId { get; set; }
        public int? WinningPlayerId { get; set; }
        public int? LosingPlayerId { get; set; }

        // Penalty Tracking
        public int? CommittedByPlayerId { get; set; }
        public int? DrawnByPlayerId { get; set; }
        public int? ServedByPlayerId { get; set; }
        public int? PenaltyDuration { get; set; }
        public string? Reason { get; set; }
        public string? SecondaryReason { get; set; }

        // Spatial & Zone Details
        public int? XCoord { get; set; }
        public int? YCoord { get; set; }
        public string? ZoneCode { get; set; }

        // Additional Granular Details & Clips
        public string? DetailsTypeCode { get; set; }
        public string? DetailsDescKey { get; set; }
        public int? PlayerId { get; set; }
        //public float? DiscreteClip { get; set; }
        //public float? DiscreteClipFr { get; set; }
        //public float? HighlightClip { get; set; }
        //public float? HighlightClipFr { get; set; }
        //public string? HighlightClipSharingUrl { get; set; }
        //public string? HighlightClipSharingUrlFr { get; set; }
    }
}
