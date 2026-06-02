using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Core.Entitties
{
    public class Franchise
    {
        public int FranchiseId { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Team> Teams { get; set; } = new List<Team>();
    }
}
