using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Domain.Entities
{
    public class PlayerAwards
    {
        public int PlayerId { get; set; }
        public int TrophyId { get; set; }
        public int SeasonId { get; set; }

        public Player Player { get; set; } = null!;
        public Trophy Trophy { get; set; } = null!;
        public Season Season { get; set; } = null!;
    }
}
