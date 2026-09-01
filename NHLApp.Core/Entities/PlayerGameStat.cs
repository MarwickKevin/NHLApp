using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Domain.Entities
{
    public class PlayerGameStat
    {
        public int Id { get; set; }
        public string? PlayerName { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int TeamId { get; set; }
        public int? SweaterNumber { get; set; }
        public string? Position { get; set; } = null!;
        public int? Goals { get; set; }
        public int? Assists { get; set; }
        public int? Points { get; set; }
        public int? PlusMinus { get; set; }
        public int? Pim { get; set; }
        public int? Hits { get; set; }
        public int? PowerPlayGoals { get; set; }
        public int? Sog { get; set; }
        public float? FaceoffWinningPctg { get; set; }
        public string? Toi { get; set; } = null!;
        public int? BlockedShots { get; set; }
        public int? Shifts { get; set; }
        public int? Giveaways { get; set; }
        public int? Takeaways { get; set; }
    }
}
