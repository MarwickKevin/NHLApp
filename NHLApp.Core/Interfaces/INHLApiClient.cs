using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Core.Interfaces
{
    public interface INHLApiClient
    {
        Task<string> GetSeasonsAsync();
        Task<string> GetTeamsAsync();
        //Task<string> GetPlayerAsync(int playerId);
        //Task<string> GetPlayByPlayAsync(int gameId);
        //Task<string> GetBoxscoreAsync(int gameId);
        Task<string> GetTeamRosterAsync(string teamCode, int seasonId);
        Task<string> GetTeamRosterSeasonsAsync(string teamCode);
    }
}
