using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Domain.Entities
{
    public class DraftDetail
    {
        public int PlayerId { get; set; }

        public int? Year { get; set; }
        public string? TeamAbbrev { get; set; } = string.Empty;
        public int? Round { get; set; }
        public int? PickInRound { get; set; }
        public int? OverallPick { get; set; }
        
        public Player Player { get; set; } = null!;
    }
}
