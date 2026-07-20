using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

