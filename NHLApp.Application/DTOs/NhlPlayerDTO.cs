using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NHLApp.Application.DTOs
{
    // Root object returned by the NHL roster API containing positional arrays
    public record NhlRosterRootDTO(
        [property: JsonPropertyName("forwards")] List<NhlPlayerDTO> Forwards,
        [property: JsonPropertyName("defensemen")] List<NhlPlayerDTO> Defensemen,
        [property: JsonPropertyName("goalies")] List<NhlPlayerDTO> Goalies
    );

    // Individual player model matching the deep nesting of the NHL JSON structure
    public record NhlPlayerDTO(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("firstName")] NhlLocalizedTextDTO FirstName,
        [property: JsonPropertyName("lastName")] NhlLocalizedTextDTO LastName

    //Unused (Implemented in PlayerLandingDTO)
    //[property: JsonPropertyName("positionCode")] string PositionCode,
    //[property: JsonPropertyName("shootsCatches")] string? ShootsCatches,
    //[property: JsonPropertyName("heightInCentimeters")] int? HeightInCentimeters,
    //[property: JsonPropertyName("weightInKilograms")] int? WeightInKilograms,
    //[property: JsonPropertyName("birthDate")] string? BirthDate,
    //[property: JsonPropertyName("birthCity")] NhlLocalizedTextDTO? BirthCity,
    //[property: JsonPropertyName("birthCountry")] string? BirthCountry,

    //[property: JsonPropertyName("headshot")] string? Headshot,
    //[property: JsonPropertyName("sweaterNumber")] int? SweaterNumber,
    //[property: JsonPropertyName("heightInInches")] int? HeightInInches,
    //[property: JsonPropertyName("weightInPounds")] int? WeightInPounds,        
    //[property: JsonPropertyName("birthStateProvince")] NhlLocalizedTextDTO? BirthStateProvince
    );

    
}

