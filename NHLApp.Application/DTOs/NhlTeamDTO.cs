namespace NHLApp.Application.DTOs
{
    // Root object returned by the NHL API containing the "data" property
    public record NhlTeamRootDTO(
        List<NhlTeamItemDTO> Data
    );

    // Object matching each individual team inside the JSON array
    public record NhlTeamItemDTO(
        int Id,
        int? FranchiseId,
        string FullName,
        string TriCode,
        string RawTricode,
        int LeagueId,
        int SeasonId
    );
}

