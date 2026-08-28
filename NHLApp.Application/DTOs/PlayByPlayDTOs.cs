using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NHLApp.Application.DTOs
{
    public record NhlPlayByPlayRootDTO(
        [property: JsonPropertyName("id")] int? Id,
        [property: JsonPropertyName("season")] int? Season,
        [property: JsonPropertyName("gameType")] int? GameType,
        [property: JsonPropertyName("limitedScoring")] bool? LimitedScoring,
        [property: JsonPropertyName("gameDate")] string? GameDate,
        [property: JsonPropertyName("venue")] NhlVenueDTO? Venue,
        [property: JsonPropertyName("venueLocation")] NhlVenueLocationDTO? VenueLocation,
        [property: JsonPropertyName("startTimeUTC")] string? StartTimeUTC,
        [property: JsonPropertyName("easternUTCOffset")] string? EasternUTCOffset,
        [property: JsonPropertyName("venueUTCOffset")] string? VenueUTCOffset,
        [property: JsonPropertyName("tvBroadcasts")] List<NhlTvBroadcastDTO>? TvBroadcasts,
        [property: JsonPropertyName("gameState")] string? GameState,
        [property: JsonPropertyName("gameScheduleState")] string? GameScheduleState,
        [property: JsonPropertyName("periodDescriptor")] NhlPeriodDescriptorDTO? PeriodDescriptor,
        [property: JsonPropertyName("awayTeam")] NhlAwayTeamDTO? AwayTeam,
        [property: JsonPropertyName("homeTeam")] NhlHomeTeamDTO? HomeTeam,
        [property: JsonPropertyName("shootoutInUse")] bool? ShootoutInUse,
        [property: JsonPropertyName("otInUse")] bool? OtInUse,
        [property: JsonPropertyName("clock")] NhlClockDTO? Clock,
        [property: JsonPropertyName("displayPeriod")] int? DisplayPeriod,
        [property: JsonPropertyName("maxPeriods")] int? MaxPeriods,
        [property: JsonPropertyName("gameOutcome")] NhlGameOutcomeDTO? GameOutcome,
        [property: JsonPropertyName("plays")] List<NhlPlayDTO>? Plays,
        [property: JsonPropertyName("rosterSpots")] List<NhlRosterSpotDTO>? RosterSpots,
        [property: JsonPropertyName("regPeriods")] int? RegPeriods,
        [property: JsonPropertyName("summary")] NhlSummaryDTO? Summary,
        [property: JsonPropertyName("specialEvent")] NhlSpecialEventDTO? SpecialEvent
    );

    public record NhlVenueDTO(
        [property: JsonPropertyName("default")] string? Default
    );

    public record NhlVenueLocationDTO(
        [property: JsonPropertyName("default")] string? Default,
        [property: JsonPropertyName("fr")] string? Fr,
        [property: JsonPropertyName("cs")] string? Cs,
        [property: JsonPropertyName("de")] string? De,
        [property: JsonPropertyName("fi")] string? Fi,
        [property: JsonPropertyName("sk")] string? Sk,
        [property: JsonPropertyName("sv")] string? Sv
    );

    public record NhlPeriodDescriptorDTO(
        [property: JsonPropertyName("number")] int? Number,
        [property: JsonPropertyName("periodType")] string? PeriodType,
        [property: JsonPropertyName("maxRegulationPeriods")] int? MaxRegulationPeriods,
        [property: JsonPropertyName("otPeriods")] int? OtPeriods
    );

    public record NhlAwayTeamDTO(
        [property: JsonPropertyName("id")] int? Id,
        [property: JsonPropertyName("commonName")] NhlCommonNameDTO? CommonName,
        [property: JsonPropertyName("abbrev")] string? Abbrev,
        [property: JsonPropertyName("score")] int? Score,
        [property: JsonPropertyName("sog")] int? Sog,
        [property: JsonPropertyName("logo")] string? Logo,
        [property: JsonPropertyName("darkLogo")] string? DarkLogo,
        [property: JsonPropertyName("placeName")] NhlPlaceNameDTO? PlaceName,
        [property: JsonPropertyName("placeNameWithPreposition")] NhlPlaceNameWithPrepositionDTO? PlaceNameWithPreposition
    );

    public record NhlCommonNameDTO(
        [property: JsonPropertyName("default")] string? Default,
        [property: JsonPropertyName("fr")] string? Fr,
        [property: JsonPropertyName("cs")] string? Cs,
        [property: JsonPropertyName("de")] string? De,
        [property: JsonPropertyName("es")] string? Es,
        [property: JsonPropertyName("fi")] string? Fi,
        [property: JsonPropertyName("sk")] string? Sk,
        [property: JsonPropertyName("sv")] string? Sv
    );

    public record NhlPlaceNameDTO(
        [property: JsonPropertyName("default")] string? Default,
        [property: JsonPropertyName("fr")] string? Fr,
        [property: JsonPropertyName("cs")] string? Cs,
        [property: JsonPropertyName("es")] string? Es,
        [property: JsonPropertyName("fi")] string? Fi,
        [property: JsonPropertyName("sk")] string? Sk,
        [property: JsonPropertyName("sv")] string? Sv
    );

    public record NhlPlaceNameWithPrepositionDTO(
        [property: JsonPropertyName("default")] string? Default,
        [property: JsonPropertyName("fr")] string? Fr,
        [property: JsonPropertyName("cs")] string? Cs,
        [property: JsonPropertyName("es")] string? Es,
        [property: JsonPropertyName("fi")] string? Fi,
        [property: JsonPropertyName("sk")] string? Sk,
        [property: JsonPropertyName("sv")] string? Sv
    );

    public record NhlHomeTeamDTO(
        [property: JsonPropertyName("id")] int? Id,
        [property: JsonPropertyName("commonName")] NhlCommonNameDTO? CommonName,
        [property: JsonPropertyName("abbrev")] string? Abbrev,
        [property: JsonPropertyName("score")] int? Score,
        [property: JsonPropertyName("sog")] int? Sog,
        [property: JsonPropertyName("logo")] string? Logo,
        [property: JsonPropertyName("darkLogo")] string? DarkLogo,
        [property: JsonPropertyName("placeName")] NhlPlaceNameDTO? PlaceName,
        [property: JsonPropertyName("placeNameWithPreposition")] NhlPlaceNameWithPrepositionDTO? PlaceNameWithPreposition
    );

    public record NhlClockDTO(
        [property: JsonPropertyName("timeRemaining")] string? TimeRemaining,
        [property: JsonPropertyName("secondsRemaining")] int? SecondsRemaining,
        [property: JsonPropertyName("running")] bool? Running,
        [property: JsonPropertyName("inIntermission")] bool? InIntermission
    );

    public record NhlGameOutcomeDTO(
        [property: JsonPropertyName("lastPeriodType")] string? LastPeriodType,
        [property: JsonPropertyName("tie")] bool? Tie,
        [property: JsonPropertyName("otPeriods")] int? OtPeriods
    );

    public record NhlPlayDTO(
        [property: JsonPropertyName("eventId")] int? EventId,
        [property: JsonPropertyName("periodDescriptor")] NhlPeriodDescriptorDTO? PeriodDescriptor,
        [property: JsonPropertyName("timeInPeriod")] string? TimeInPeriod,
        [property: JsonPropertyName("timeRemaining")] string? TimeRemaining,
        [property: JsonPropertyName("typeCode")] int? TypeCode,
        [property: JsonPropertyName("typeDescKey")] string? TypeDescKey,
        [property: JsonPropertyName("sortOrder")] int? SortOrder,
        [property: JsonPropertyName("details")] NhlDetailsDTO? Details,
        [property: JsonPropertyName("situationCode")] string? SituationCode,
        [property: JsonPropertyName("homeTeamDefendingSide")] string? HomeTeamDefendingSide,
        [property: JsonPropertyName("pptReplayUrl")] string? PptReplayUrl
    );

    public record NhlDetailsDTO(
        [property: JsonPropertyName("typeCode")] string? TypeCode,
        [property: JsonPropertyName("descKey")] string? DescKey,
        [property: JsonPropertyName("duration")] int? Duration,
        [property: JsonPropertyName("committedByPlayerId")] int? CommittedByPlayerId,
        [property: JsonPropertyName("eventOwnerTeamId")] int? EventOwnerTeamId,
        [property: JsonPropertyName("scoringPlayerId")] int? ScoringPlayerId,
        [property: JsonPropertyName("scoringPlayerTotal")] int? ScoringPlayerTotal,
        [property: JsonPropertyName("assist1PlayerId")] int? Assist1PlayerId,
        [property: JsonPropertyName("assist1PlayerTotal")] int? Assist1PlayerTotal,
        [property: JsonPropertyName("goalieInNetId")] int? GoalieInNetId,
        [property: JsonPropertyName("awayScore")] int? AwayScore,
        [property: JsonPropertyName("homeScore")] int? HomeScore,
        [property: JsonPropertyName("assist2PlayerId")] int? Assist2PlayerId,
        [property: JsonPropertyName("assist2PlayerTotal")] int? Assist2PlayerTotal,
        [property: JsonPropertyName("goalInGame")] int? GoalInGame,
        [property: JsonPropertyName("servedByPlayerId")] int? ServedByPlayerId,
        [property: JsonPropertyName("shotType")] string? ShotType,
        [property: JsonPropertyName("shootingPlayerId")] int? ShootingPlayerId,
        [property: JsonPropertyName("awaySOG")] int? AwaySOG,
        [property: JsonPropertyName("homeSOG")] int? HomeSOG,
        [property: JsonPropertyName("drawnByPlayerId")] int? DrawnByPlayerId,
        [property: JsonPropertyName("losingPlayerId")] int? LosingPlayerId,
        [property: JsonPropertyName("winningPlayerId")] int? WinningPlayerId,
        [property: JsonPropertyName("xCoord")] int? XCoord,
        [property: JsonPropertyName("yCoord")] int? YCoord,
        [property: JsonPropertyName("reason")] string? Reason,
        [property: JsonPropertyName("hittingPlayerId")] int? HittingPlayerId,
        [property: JsonPropertyName("hitteePlayerId")] int? HitteePlayerId,
        [property: JsonPropertyName("zoneCode")] string? ZoneCode,
        [property: JsonPropertyName("playerId")] int? PlayerId,
        [property: JsonPropertyName("blockingPlayerId")] int? BlockingPlayerId,
        [property: JsonPropertyName("secondaryReason")] string? SecondaryReason,
        [property: JsonPropertyName("discreteClip")] float? DiscreteClip,
        [property: JsonPropertyName("highlightClipSharingUrl")] string? HighlightClipSharingUrl,
        [property: JsonPropertyName("highlightClip")] float? HighlightClip,
        [property: JsonPropertyName("highlightClipSharingUrlFr")] string? HighlightClipSharingUrlFr,
        [property: JsonPropertyName("highlightClipFr")] float? HighlightClipFr,
        [property: JsonPropertyName("discreteClipFr")] float? DiscreteClipFr,
        [property: JsonPropertyName("assist3PlayerId")] int? Assist3PlayerId,
        [property: JsonPropertyName("assist3PlayerTotal")] int? Assist3PlayerTotal
    );

    public record NhlRosterSpotDTO(
        [property: JsonPropertyName("teamId")] int? TeamId,
        [property: JsonPropertyName("playerId")] int? PlayerId,
        [property: JsonPropertyName("firstName")] NhlFirstNameDTO? FirstName,
        [property: JsonPropertyName("lastName")] NhlLastNameDTO? LastName,
        [property: JsonPropertyName("sweaterNumber")] int? SweaterNumber,
        [property: JsonPropertyName("positionCode")] string? PositionCode,
        [property: JsonPropertyName("headshot")] string? Headshot
    );

    public record NhlFirstNameDTO(
        [property: JsonPropertyName("default")] string? Default,
        [property: JsonPropertyName("cs")] string? Cs,
        [property: JsonPropertyName("de")] string? De,
        [property: JsonPropertyName("fi")] string? Fi,
        [property: JsonPropertyName("sk")] string? Sk,
        [property: JsonPropertyName("sv")] string? Sv,
        [property: JsonPropertyName("es")] string? Es,
        [property: JsonPropertyName("fr")] string? Fr
    );

    public record NhlLastNameDTO(
        [property: JsonPropertyName("default")] string? Default,
        [property: JsonPropertyName("cs")] string? Cs,
        [property: JsonPropertyName("sk")] string? Sk,
        [property: JsonPropertyName("de")] string? De,
        [property: JsonPropertyName("es")] string? Es,
        [property: JsonPropertyName("fi")] string? Fi,
        [property: JsonPropertyName("sv")] string? Sv,
        [property: JsonPropertyName("fr")] string? Fr
    );

    public record NhlSummaryDTO(
    );

    public record NhlSpecialEventDTO(
        [property: JsonPropertyName("parentId")] int? ParentId,
        [property: JsonPropertyName("name")] NhlNameDTO? Name,
        [property: JsonPropertyName("lightLogoUrl")] NhlLightLogoUrlDTO? LightLogoUrl
    );

    public record NhlNameDTO(
        [property: JsonPropertyName("default")] string? Default,
        [property: JsonPropertyName("fr")] string? Fr,
        [property: JsonPropertyName("sk")] string? Sk,
        [property: JsonPropertyName("sv")] string? Sv
    );

    public record NhlTvBroadcastDTO(
        [property: JsonPropertyName("id")] int? Id,
        [property: JsonPropertyName("market")] string? Market,
        [property: JsonPropertyName("countryCode")] string? CountryCode,
        [property: JsonPropertyName("network")] string? Network,
        [property: JsonPropertyName("sequenceNumber")] int? SequenceNumber
    );

    public record NhlLightLogoUrlDTO(
        [property: JsonPropertyName("default")] string? Default,
        [property: JsonPropertyName("fr")] string? Fr
    );

}
