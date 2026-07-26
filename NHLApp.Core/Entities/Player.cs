using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Domain.Entities
{
    public class Player
    {
        public int PlayerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string ShootsCatches { get; set; } = string.Empty;
        public DateOnly? BirthDate { get; set; }
        public string? BirthCity { get; set; }
        public string? BirthCountry { get; set; }
        public int? HeightInCentimeters { get; set; }
        public int? WeightInKilograms { get; set; }

        public string? Headshot { get; set; }
        public int? SweaterNumber { get; set; }
        public int? HeightInInches { get; set; }
        public int? WeightInPounds { get; set; }
        public string? BirthStateProvince { get; set; }



        //[property: JsonPropertyName("headshot")]
        //string? Headshot,
        //[property: JsonPropertyName("sweaterNumber")] int? SweaterNumber,
        //[property: JsonPropertyName("heightInInches")] int? HeightInInches,
        //[property: JsonPropertyName("weightInPounds")] int? WeightInPounds,
        //[property: JsonPropertyName("birthCountry")] string? BirthCountry,
        //[property: JsonPropertyName("birthStateProvince")] NhlLocalizedTextDTO? BirthStateProvince
    }
}
