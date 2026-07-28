using System.Text.Json.Serialization;

namespace NHLApp.Application.DTOs
{
    // Root object returned by the NHL API containing the "data" array and total count
    public record NhlTeamRootDTO(
        [property: JsonPropertyName("data")] List<NhlTeamItemDTO>? Data,
        [property: JsonPropertyName("total")] int Total
    );

    // Object matching each individual team inside the JSON array
    public record NhlTeamItemDTO(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("franchiseId")] int? FranchiseId,
        [property: JsonPropertyName("fullName")] string FullName,
        [property: JsonPropertyName("triCode")] string TriCode,
        [property: JsonPropertyName("rawTricode")] string RawTricode,
        [property: JsonPropertyName("leagueId")] int LeagueId
    );
}