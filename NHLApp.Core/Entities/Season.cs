using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Domain.Entities
{
    public class Season
    {
        public int SeasonId { get; set; }
        public int StartYear { get; set; }
        public int EndYear { get; set; }
    }
}
