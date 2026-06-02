using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Core.Entitties
{
    public class RawApiResponse
    {
        public int Id { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;
        public DateTime FetchedAt { get; set; }
    }
}
