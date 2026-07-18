using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Application.DTOs
{
    // Root object returned by the NHL roster API containing positional arrays
    public record NhlRosterRootDTO(
        List<NhlPlayerDTO> Forwards,
        List<NhlPlayerDTO> Defensemen,
        List<NhlPlayerDTO> Goalies
    );

    // Individual player model matching the deep nesting of the NHL JSON structure
    public record NhlPlayerDTO(
        int Id,
        NhlLocalizedTextDTO FirstName,
        NhlLocalizedTextDTO LastName,
        string PositionCode,
        string? ShootsCatches,
        int? HeightInCentimeters,
        int? WeightInKilograms,
        string? BirthDate,
        NhlLocalizedTextDTO? BirthCity,
        string? BirthCountry
    );

    // Reusable DTO for the NHL API localized text objects (e.g., {"default": "Montreal"})
    public record NhlLocalizedTextDTO(
        string Default
    );
}

