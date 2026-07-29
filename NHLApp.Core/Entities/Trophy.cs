using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Domain.Entities
{
    public class Trophy
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Trophy";

        public ICollection<PlayerAwards> Awards { get; set; } = new HashSet<PlayerAwards>();
    }
}
