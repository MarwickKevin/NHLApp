using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NHLApp.Application.DTOs.BoxscoreDTOs
{
    public record NhlBoxscoreRootDTO(
        [property: JsonPropertyName("id")] int? Id,
        [property: JsonPropertyName("season")] int? Season,
        [property: JsonPropertyName("gameType")] int? GameType,
        [property: JsonPropertyName("limitedScoring")] bool? LimitedScoring,
        [property: JsonPropertyName("gameDate")] string? GameDate,
        [property: JsonPropertyName("venue")] NhlVenueDTO? Venue,
        [property: JsonPropertyName("venueLocation")] NhlVenueLocationDTO? VenueLocation,
        [property: JsonPropertyName("startTimeUTC")] string? StartTimeUTC,
        //[property: JsonPropertyName("easternUTCOffset")] string? EasternUTCOffset,
        //[property: JsonPropertyName("venueUTCOffset")] string? VenueUTCOffset,
        //[property: JsonPropertyName("tvBroadcasts")] List<NhlTvBroadcastDTO>? TvBroadcasts,
        [property: JsonPropertyName("gameState")] string? GameState,
        [property: JsonPropertyName("gameScheduleState")] string? GameScheduleState,
        [property: JsonPropertyName("periodDescriptor")] NhlPeriodDescriptorDTO? PeriodDescriptor,
        [property: JsonPropertyName("regPeriods")] int? RegPeriods,
        [property: JsonPropertyName("awayTeam")] NhlAwayTeamDTO? AwayTeam,
        [property: JsonPropertyName("homeTeam")] NhlHomeTeamDTO? HomeTeam,
        [property: JsonPropertyName("clock")] NhlClockDTO? Clock,
        [property: JsonPropertyName("playerByGameStats")] NhlPlayerByGameStatsDTO? PlayerByGameStats,
        [property: JsonPropertyName("gameOutcome")] NhlGameOutcomeDTO? GameOutcome,
        [property: JsonPropertyName("specialEvent")] NhlSpecialEventDTO? SpecialEvent
    );

    public record NhlVenueDTO(
        [property: JsonPropertyName("default")] string? Default
    );

    public record NhlVenueLocationDTO(
        [property: JsonPropertyName("default")] string? Default
        //[property: JsonPropertyName("fr")] string? Fr,
        //[property: JsonPropertyName("sk")] string? Sk,
        //[property: JsonPropertyName("cs")] string? Cs,
        //[property: JsonPropertyName("de")] string? De,
        //[property: JsonPropertyName("fi")] string? Fi,
        //[property: JsonPropertyName("sv")] string? Sv
    );

    //public record NhlTvBroadcastDTO(
    //    [property: JsonPropertyName("id")] int? Id,
    //    [property: JsonPropertyName("market")] string? Market,
    //    [property: JsonPropertyName("countryCode")] string? CountryCode,
    //    [property: JsonPropertyName("network")] string? Network,
    //    [property: JsonPropertyName("sequenceNumber")] int? SequenceNumber
    //);

    public record NhlPeriodDescriptorDTO(
        [property: JsonPropertyName("number")] int? Number,
        [property: JsonPropertyName("periodType")] string? PeriodType,
        [property: JsonPropertyName("maxRegulationPeriods")] int? MaxRegulationPeriods,
        [property: JsonPropertyName("otPeriods")] int? OtPeriods
    );

    public record NhlAwayTeamDTO(
        [property: JsonPropertyName("id")] int? Id,
        //[property: JsonPropertyName("commonName")] NhlCommonNameDTO? CommonName,
        //[property: JsonPropertyName("abbrev")] string? Abbrev,
        [property: JsonPropertyName("score")] int? Score,
        [property: JsonPropertyName("sog")] int? Sog,
        //[property: JsonPropertyName("logo")] string? Logo,
        //[property: JsonPropertyName("darkLogo")] string? DarkLogo,
        //[property: JsonPropertyName("placeName")] NhlPlaceNameDTO? PlaceName,
        //[property: JsonPropertyName("placeNameWithPreposition")] NhlPlaceNameWithPrepositionDTO? PlaceNameWithPreposition,
        [property: JsonPropertyName("forwards")] List<NhlForwardDTO>? Forwards,
        [property: JsonPropertyName("defense")] List<NhlDefenseDTO>? Defense,
        [property: JsonPropertyName("goalies")] List<NhlGoalieDTO>? Goalies
    );

    public record NhlCommonNameDTO(
        [property: JsonPropertyName("default")] string? Default
        //[property: JsonPropertyName("fr")] string? Fr,
        //[property: JsonPropertyName("cs")] string? Cs,
        //[property: JsonPropertyName("de")] string? De,
        //[property: JsonPropertyName("es")] string? Es,
        //[property: JsonPropertyName("fi")] string? Fi,
        //[property: JsonPropertyName("sk")] string? Sk,
        //[property: JsonPropertyName("sv")] string? Sv
    );

    public record NhlPlaceNameDTO(
        [property: JsonPropertyName("default")] string? Default
        //[property: JsonPropertyName("fr")] string? Fr,
        //[property: JsonPropertyName("cs")] string? Cs,
        //[property: JsonPropertyName("es")] string? Es,
        //[property: JsonPropertyName("fi")] string? Fi,
        //[property: JsonPropertyName("sk")] string? Sk,
        //[property: JsonPropertyName("sv")] string? Sv
    );

    public record NhlPlaceNameWithPrepositionDTO(
        [property: JsonPropertyName("default")] string? Default
        //[property: JsonPropertyName("fr")] string? Fr,
        //[property: JsonPropertyName("cs")] string? Cs,
        //[property: JsonPropertyName("es")] string? Es,
        //[property: JsonPropertyName("fi")] string? Fi,
        //[property: JsonPropertyName("sk")] string? Sk,
        //[property: JsonPropertyName("sv")] string? Sv
    );

    public record NhlHomeTeamDTO(
        [property: JsonPropertyName("id")] int? Id,
        //[property: JsonPropertyName("commonName")] NhlCommonNameDTO? CommonName,
        //[property: JsonPropertyName("abbrev")] string? Abbrev,
        [property: JsonPropertyName("score")] int? Score,
        [property: JsonPropertyName("sog")] int? Sog,
        //[property: JsonPropertyName("logo")] string? Logo,
        //[property: JsonPropertyName("darkLogo")] string? DarkLogo,
        //[property: JsonPropertyName("placeName")] NhlPlaceNameDTO? PlaceName,
        //[property: JsonPropertyName("placeNameWithPreposition")] NhlPlaceNameWithPrepositionDTO? PlaceNameWithPreposition,
        [property: JsonPropertyName("forwards")] List<NhlForwardDTO>? Forwards,
        [property: JsonPropertyName("defense")] List<NhlDefenseDTO>? Defense,
        [property: JsonPropertyName("goalies")] List<NhlGoalieDTO>? Goalies
    );

    public record NhlClockDTO(
        [property: JsonPropertyName("timeRemaining")] string? TimeRemaining,
        [property: JsonPropertyName("secondsRemaining")] int? SecondsRemaining,
        [property: JsonPropertyName("running")] bool? Running,
        [property: JsonPropertyName("inIntermission")] bool? InIntermission
    );

    public record NhlPlayerByGameStatsDTO(
        [property: JsonPropertyName("awayTeam")] NhlAwayTeamDTO? AwayTeam,
        [property: JsonPropertyName("homeTeam")] NhlHomeTeamDTO? HomeTeam
    );

    public record NhlForwardDTO(
        [property: JsonPropertyName("playerId")] int? PlayerId,
        [property: JsonPropertyName("sweaterNumber")] int? SweaterNumber,
        [property: JsonPropertyName("name")] NhlNameDTO? Name,
        [property: JsonPropertyName("position")] string? Position,
        [property: JsonPropertyName("goals")] int? Goals,
        [property: JsonPropertyName("assists")] int? Assists,
        [property: JsonPropertyName("points")] int? Points,
        [property: JsonPropertyName("plusMinus")] int? PlusMinus,
        [property: JsonPropertyName("pim")] int? Pim,
        [property: JsonPropertyName("hits")] int? Hits,
        [property: JsonPropertyName("powerPlayGoals")] int? PowerPlayGoals,
        [property: JsonPropertyName("sog")] int? Sog,
        [property: JsonPropertyName("faceoffWinningPctg")] float? FaceoffWinningPctg,
        [property: JsonPropertyName("toi")] string? Toi,
        [property: JsonPropertyName("blockedShots")] int? BlockedShots,
        [property: JsonPropertyName("shifts")] int? Shifts,
        [property: JsonPropertyName("giveaways")] int? Giveaways,
        [property: JsonPropertyName("takeaways")] int? Takeaways
    );

    public record NhlNameDTO(
        [property: JsonPropertyName("default")] string? Default
        //[property: JsonPropertyName("cs")] string? Cs,
        //[property: JsonPropertyName("fi")] string? Fi,
        //[property: JsonPropertyName("sk")] string? Sk,
        //[property: JsonPropertyName("sv")] string? Sv,
        //[property: JsonPropertyName("de")] string? De,
        //[property: JsonPropertyName("es")] string? Es,
        //[property: JsonPropertyName("fr")] string? Fr
    );

    public record NhlDefenseDTO(
        [property: JsonPropertyName("playerId")] int? PlayerId,
        [property: JsonPropertyName("sweaterNumber")] int? SweaterNumber,
        [property: JsonPropertyName("name")] NhlNameDTO? Name,
        [property: JsonPropertyName("position")] string? Position,
        [property: JsonPropertyName("goals")] int? Goals,
        [property: JsonPropertyName("assists")] int? Assists,
        [property: JsonPropertyName("points")] int? Points,
        [property: JsonPropertyName("plusMinus")] int? PlusMinus,
        [property: JsonPropertyName("pim")] int? Pim,
        [property: JsonPropertyName("hits")] int? Hits,
        [property: JsonPropertyName("powerPlayGoals")] int? PowerPlayGoals,
        [property: JsonPropertyName("sog")] int? Sog,
        [property: JsonPropertyName("faceoffWinningPctg")] float? FaceoffWinningPctg,
        [property: JsonPropertyName("toi")] string? Toi,
        [property: JsonPropertyName("blockedShots")] int? BlockedShots,
        [property: JsonPropertyName("shifts")] int? Shifts,
        [property: JsonPropertyName("giveaways")] int? Giveaways,
        [property: JsonPropertyName("takeaways")] int? Takeaways
    );

    public record NhlGoalieDTO(
        [property: JsonPropertyName("playerId")] int? PlayerId,
        [property: JsonPropertyName("sweaterNumber")] int? SweaterNumber,
        [property: JsonPropertyName("name")] NhlNameDTO? Name,
        [property: JsonPropertyName("position")] string? Position,
        [property: JsonPropertyName("evenStrengthShotsAgainst")] string? EvenStrengthShotsAgainst,
        [property: JsonPropertyName("powerPlayShotsAgainst")] string? PowerPlayShotsAgainst,
        [property: JsonPropertyName("shorthandedShotsAgainst")] string? ShorthandedShotsAgainst,
        [property: JsonPropertyName("saveShotsAgainst")] string? SaveShotsAgainst,
        [property: JsonPropertyName("evenStrengthGoalsAgainst")] int? EvenStrengthGoalsAgainst,
        [property: JsonPropertyName("powerPlayGoalsAgainst")] int? PowerPlayGoalsAgainst,
        [property: JsonPropertyName("shorthandedGoalsAgainst")] int? ShorthandedGoalsAgainst,
        [property: JsonPropertyName("pim")] int? Pim,
        [property: JsonPropertyName("goalsAgainst")] int? GoalsAgainst,
        [property: JsonPropertyName("toi")] string? Toi,
        [property: JsonPropertyName("starter")] bool? Starter,
        [property: JsonPropertyName("shotsAgainst")] int? ShotsAgainst,
        [property: JsonPropertyName("saves")] int? Saves,
        [property: JsonPropertyName("savePctg")] float? SavePctg,
        [property: JsonPropertyName("decision")] string? Decision
    );

    public record NhlGameOutcomeDTO(
        [property: JsonPropertyName("lastPeriodType")] string? LastPeriodType,
        [property: JsonPropertyName("otPeriods")] int? OtPeriods,
        [property: JsonPropertyName("tie")] bool? Tie
    );

    public record NhlSpecialEventDTO(
        [property: JsonPropertyName("parentId")] int? ParentId,
        [property: JsonPropertyName("name")] NhlNameDTO? Name,
        [property: JsonPropertyName("lightLogoUrl")] NhlLightLogoUrlDTO? LightLogoUrl
    );

    public record NhlLightLogoUrlDTO(
        [property: JsonPropertyName("default")] string? Default
        //[property: JsonPropertyName("fr")] string? Fr
    );
}
