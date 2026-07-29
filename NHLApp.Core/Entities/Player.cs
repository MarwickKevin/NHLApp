using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Domain.Entities
{
    public class Player
    {
        // Properties from transform team-roster
        public int PlayerId { get; set; }
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;

        // Properties currently in transform team-roster (need to be taken out, to be used in transform player-landings only)
        public string? Headshot { get; set; }

        public int? SweaterNumber { get; set; }
        public string? Position { get; set; } = string.Empty;
        public string? ShootsCatches { get; set; } = string.Empty;

        public DateOnly? BirthDate { get; set; }
        public string? BirthCity { get; set; }
        public string? BirthStateProvince { get; set; }
        public string? BirthCountry { get; set; }

        public int? HeightInCentimeters { get; set; }
        public int? WeightInKilograms { get; set; }
        public int? HeightInInches { get; set; }
        public int? WeightInPounds { get; set; }

        // Added properties from player-landings
        public bool? IsActive { get; set; }

        public int? CurrentTeamId { get; set; }
        public string? CurrentTeamAbbrev { get; set; }
        public string? FullTeamName { get; set; }
        public string? TeamCommonName { get; set; }
        public string? TeamPlaceNameWithPreposition {  get; set; }
        public string? TeamLogo {  get; set; }

        public string? HeroImage { get; set; }
        public string? PlayerSlug { get; set; }

        // Added navigation properties

        public DraftDetail? DraftDetail { get; set; }
        public ICollection<SeasonTotal> SeasonTotal { get; set; } = new HashSet<SeasonTotal>();
        public ICollection<PlayerAwards> PlayerAwards { get; set; } = new HashSet<PlayerAwards>();
    }
}
