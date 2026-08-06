using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Application.DTOs
{
    public record SeasonInfo(int SeasonId)
    {
        public DateTime StartDate => new DateTime(SeasonId / 10000, 10, 1);
        public DateTime EndDate => new DateTime(SeasonId % 10000, 6, 30);
    }
}
