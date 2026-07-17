using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Core.Entities
{
    public class Team
    {
        public int TeamId { get; set; }
        public int? FranchiseId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string TriCode { get; set; } = string.Empty;
        public string RawTriCode { get; set; } = string.Empty;
        public int LeagueId { get; set; }

        public Franchise? Franchise { get; set; }
    }
}
