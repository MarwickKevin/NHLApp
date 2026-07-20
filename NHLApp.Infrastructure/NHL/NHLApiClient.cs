using NHLApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Infrastructure.NHL
{
    public class NHLApiClient : INHLApiClient
    {
        private readonly HttpClient _httpClient;
        private const string WebApiBase = "https://api-web.nhle.com/v1/";
        private const string StatsApiBase = "https://api.nhle.com/stats/rest/en/";

        public NHLApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetSeasonsAsync()
            => await _httpClient.GetStringAsync($"{WebApiBase}season");

        public async Task<string> GetTeamsAsync()
            => await _httpClient.GetStringAsync($"{StatsApiBase}team");

        public async Task<string> GetTeamRosterAsync(string teamCode, int seasonId)
             => await _httpClient.GetStringAsync($"{WebApiBase}roster/{teamCode}/{seasonId}");

        public async Task<string> GetTeamRosterSeasonsAsync(string teamCode)
            => await _httpClient.GetStringAsync($"{WebApiBase}roster-season/{teamCode}");

        //public async Task<string> GetPlayerAsync(int playerId)
        //    => await _httpClient.GetStringAsync($"{WebApiBase}player/{playerId}/landing");

        //public async Task<string> GetPlayByPlayAsync(int gameId)
        //    => await _httpClient.GetStringAsync($"{WebApiBase}gamecenter/{gameId}/play-by-play");

        //public async Task<string> GetBoxscoreAsync(int gameId)
        //    => await _httpClient.GetStringAsync($"{WebApiBase}gamecenter/{gameId}/boxscore");
    }
}
