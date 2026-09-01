using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Domain.Entities
{
    public class GoalieGameStat
    {
        public int Id { get; set; }
        public string? PlayerName { get; set; }
        public string? Position { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int TeamId { get; set; }
        public int? SweaterNumber { get; set; }
        public string? Toi { get; set; } = null!;
        public bool? Starter { get; set; }
        public int? ShotsAgainst { get; set; }
        public int? Saves { get; set; }
        public int? GoalsAgainst { get; set; }
        public float? SavePctg { get; set; }
        public int? Pim { get; set; }
        public string? Decision { get; set; }
        public string? EvenStrengthShotsAgainst { get; set; } = null!;
        public string? PowerPlayShotsAgainst { get; set; } = null!;
        public string? ShorthandedShotsAgainst { get; set; } = null!;
        public string? SaveShotsAgainst { get; set; } = null!;
        public int? EvenStrengthGoalsAgainst { get; set; }
        public int? PowerPlayGoalsAgainst { get; set; }
        public int? ShorthandedGoalsAgainst { get; set; }
    }
}
