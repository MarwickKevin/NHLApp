using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Application.Contexts
{
    public class WorkerContext
    {
        public DateTime StartedAt { get; } = DateTime.UtcNow;

        public DateTime? FinishedAt { get; set; }

        public int TotalApiCalls { get; set; }

        public int ApiCallsErrors { get; set; }

        public HashSet<int> PlayerIds { get; } = new();

        public TimeSpan Duration => (FinishedAt ?? DateTime.UtcNow) - StartedAt;
    }
}
