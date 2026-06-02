using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Core.Entitties
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
    }
}
