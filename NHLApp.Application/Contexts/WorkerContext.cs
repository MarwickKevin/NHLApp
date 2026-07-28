using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Application.Contexts
{
    public class WorkerContext
    {
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;  //

        public DateTime? FinishedAt { get; set; } //

        public int TotalApiCalls { get; set; } //

        public int TotalImportErrors { get; set; } = 0; // 

        public int TotalTransformErrors { get; set; } = 0; 

        public TimeSpan Duration => (FinishedAt ?? DateTime.UtcNow) - StartedAt; 

        public HashSet<int> PlayerIds { get; } = new();

    }
}
