using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NHLApp.Domain.Entities
{
    public class SeasonTotal
    {
        public int Id { get; set; }
        public int PlayerId { get; set; } // FK        

        public int? Assists { get; set; }
        public int? GameTypeId { get; set; }
        public int? GamesPlayed { get; set; }
        public int? Goals { get; set; }
        public string? LeagueAbbrev { get; set; }
        public int? Pim {  get; set; }
        public int? Points { get; set; }
        public int? Season {  get; set; }
        public int? Sequence { get; set; }
        public string? TeamName { get; set; }
        public int? GameWinningGoals { get; set; }
        public int? PlusMinus { get; set; }
        public int? PowerPlayGoals { get; set; }
        public int? ShorthandedGoals { get; set; }
        public int? Shots { get; set; }
        public string? TeamCommonName { get; set; }
        public string? TeamPlaceNameWithPreposition { get; set; }
        public string? AvgToi { get; set; }
        public float? FaceoffWinningPctg { get; set; }
        public int? OtGoals { get; set; }
        public int? PowerPlayPoints { get; set; }
        public float? ShootingPctg { get; set; }
        public int? ShorthandedPoints { get; set; }

        public Player Player { get; set; } = null!;

    }
}
