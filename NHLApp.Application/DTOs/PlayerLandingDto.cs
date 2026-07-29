using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NHLApp.Application.DTOs
{
    public record NhlPlayerLandingDTO(
         //Already tranformed properties (from transform TeamRoster)
         [property: JsonPropertyName("playerId")] int PlayerId,
         [property: JsonPropertyName("firstName")] NhlLocalizedTextDTO? FirstName,
         [property: JsonPropertyName("lastName")] NhlLocalizedTextDTO? LastName,

         // TODO: Properties transformed in team-roster that need to be removed
         [property: JsonPropertyName("headshot")] string? Headshot,

         [property: JsonPropertyName("sweaterNumber")] int? SweaterNumber,
         [property: JsonPropertyName("position")] string? Position,
         [property: JsonPropertyName("shootsCatches")] string? ShootsCatches,

         [property: JsonPropertyName("birthDate")] string? BirthDate,
         [property: JsonPropertyName("birthCity")] NhlLocalizedTextDTO? BirthCity,
         [property: JsonPropertyName("birthStateProvince")] NhlLocalizedTextDTO? BirthStateProvince,
         [property: JsonPropertyName("birthCountry")] string? BirthCountry,

         [property: JsonPropertyName("heightInInches")] int? HeightInInches,
         [property: JsonPropertyName("heightInCentimeters")] int? HeightInCentimeters,
         [property: JsonPropertyName("weightInPounds")] int? WeightInPounds,
         [property: JsonPropertyName("weightInKilograms")] int? WeightInKilograms,

         //New Players properties 
         [property: JsonPropertyName("isActive")] bool? IsActive,

         [property: JsonPropertyName("currentTeamId")] int? CurrentTeamId,
         [property: JsonPropertyName("currentTeamAbbrev")] string? CurrentTeamAbbrev,
         [property: JsonPropertyName("fullTeamName")] NhlLocalizedTextDTO? FullTeamName,
         [property: JsonPropertyName("teamCommonName")] NhlLocalizedTextDTO? TeamCommonName,
         [property: JsonPropertyName("teamPlaceNameWithPreposition")] NhlLocalizedTextDTO? TeamPlaceNameWithPreposition,
         [property: JsonPropertyName("teamLogo")] string? TeamLogo,

         [property: JsonPropertyName("heroImage")] string? HeroImage,
         [property: JsonPropertyName("playerSlug")] string? PlayerSlug,

         //Navigation properties (Players)
         [property: JsonPropertyName("draftDetails")] NhlDraftDetailsDTO? DraftDetails,
         [property: JsonPropertyName("seasonTotals")] List<NhlSeasonTotalDTO>? SeasonTotals,
         [property: JsonPropertyName("awards")] List<NhlAwardDTO>? Awards

         ////Unused social media properties
         //[property: JsonPropertyName("shopLink")] string? ShopLink,
         //[property: JsonPropertyName("twitterLink")] string? TwitterLink,
         //[property: JsonPropertyName("watchLink")] string? WatchLink,

         ////Unused navigation property
         //[property: JsonPropertyName("currentTeamRoster")] List<NhlCurrentTeamRosterDTO>? CurrentTeamRoster
         //[property: JsonPropertyName("last5Games")] List<NhlLast5GameDTO>? Last5Games,
     );

    // New table
    public record NhlDraftDetailsDTO(
        [property: JsonPropertyName("year")] int? Year,
        [property: JsonPropertyName("teamAbbrev")] string? TeamAbbrev,
        [property: JsonPropertyName("round")] int? Round,
        [property: JsonPropertyName("pickInRound")] int? PickInRound,
        [property: JsonPropertyName("overallPick")] int? OverallPick
    );    

    // New table
    public record NhlSeasonTotalDTO(
        [property: JsonPropertyName("assists")] int? Assists,
        [property: JsonPropertyName("gameTypeId")] int? GameTypeId,
        [property: JsonPropertyName("gamesPlayed")] int? GamesPlayed,
        [property: JsonPropertyName("goals")] int? Goals,
        [property: JsonPropertyName("leagueAbbrev")] string? LeagueAbbrev,
        [property: JsonPropertyName("pim")] int? Pim,
        [property: JsonPropertyName("points")] int? Points,
        [property: JsonPropertyName("season")] int? Season,
        [property: JsonPropertyName("sequence")] int? Sequence,
        [property: JsonPropertyName("teamName")] NhlLocalizedTextDTO? TeamName,
        [property: JsonPropertyName("gameWinningGoals")] int? GameWinningGoals,
        [property: JsonPropertyName("plusMinus")] int? PlusMinus,
        [property: JsonPropertyName("powerPlayGoals")] int? PowerPlayGoals,
        [property: JsonPropertyName("shorthandedGoals")] int? ShorthandedGoals,
        [property: JsonPropertyName("shots")] int? Shots,
        [property: JsonPropertyName("teamCommonName")] NhlLocalizedTextDTO? TeamCommonName,
        [property: JsonPropertyName("teamPlaceNameWithPreposition")] NhlLocalizedTextDTO? TeamPlaceNameWithPreposition,
        [property: JsonPropertyName("avgToi")] string? AvgToi,
        [property: JsonPropertyName("faceoffWinningPctg")] float? FaceoffWinningPctg,
        [property: JsonPropertyName("otGoals")] int? OtGoals,
        [property: JsonPropertyName("powerPlayPoints")] int? PowerPlayPoints,
        [property: JsonPropertyName("shootingPctg")] float? ShootingPctg,
        [property: JsonPropertyName("shorthandedPoints")] int? ShorthandedPoints
    );

    // Links PlayerAwards to seasons
    public record NhlAwardSeasonDTO(
        [property: JsonPropertyName("seasonId")] int SeasonId
    );

    // Used to populate Trophies and PlayerAwards
    public record NhlAwardDTO(
        [property: JsonPropertyName("trophy")] NhlLocalizedTextDTO Trophy,
        [property: JsonPropertyName("seasons")] List<NhlAwardSeasonDTO> Seasons
    );
    
    // Unused (availlable through NhlSeasonTotal
    //    [property: JsonPropertyName("assists")] int Assists,
    //    [property: JsonPropertyName("blockedShots")] int BlockedShots,
    //    [property: JsonPropertyName("gameTypeId")] int GameTypeId,
    //    [property: JsonPropertyName("gamesPlayed")] int GamesPlayed,
    //    [property: JsonPropertyName("goals")] int Goals,
    //    [property: JsonPropertyName("hits")] int Hits,
    //    [property: JsonPropertyName("pim")] int Pim,
    //    [property: JsonPropertyName("plusMinus")] int PlusMinus,
    //    [property: JsonPropertyName("points")] int Points


    //// Unused (current team roster available through TeamRosters)
    //public record NhlCurrentTeamRosterDTO(
    //    [property: JsonPropertyName("playerId")] int PlayerId,
    //    [property: JsonPropertyName("lastName")] NhlLocalizedTextDTO LastName,
    //    [property: JsonPropertyName("firstName")] NhlLocalizedTextDTO FirstName,
    //    [property: JsonPropertyName("playerSlug")] string PlayerSlug
    //);

    //// Unused 
    //public record NhlLast5GameDTO(
    //    [property: JsonPropertyName("assists")] int Assists,
    //    [property: JsonPropertyName("gameDate")] string GameDate,
    //    [property: JsonPropertyName("gameId")] int GameId,
    //    [property: JsonPropertyName("gameTypeId")] int GameTypeId,
    //    [property: JsonPropertyName("goals")] int Goals,
    //    [property: JsonPropertyName("homeRoadFlag")] string HomeRoadFlag,
    //    [property: JsonPropertyName("opponentAbbrev")] string OpponentAbbrev,
    //    [property: JsonPropertyName("pim")] int Pim,
    //    [property: JsonPropertyName("plusMinus")] int PlusMinus,
    //    [property: JsonPropertyName("points")] int Points,
    //    [property: JsonPropertyName("powerPlayGoals")] int PowerPlayGoals,
    //    [property: JsonPropertyName("shifts")] int Shifts,
    //    [property: JsonPropertyName("shorthandedGoals")] int ShorthandedGoals,
    //    [property: JsonPropertyName("shots")] int Shots,
    //    [property: JsonPropertyName("teamAbbrev")] string TeamAbbrev,
    //    [property: JsonPropertyName("toi")] string Toi
    //);
}
