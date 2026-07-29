using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NHLApp.Application.DTOs
{
    // Reusable DTO for the NHL API localized text objects (e.g., {"default": "Montreal"})
    public record NhlLocalizedTextDTO(
        [property: JsonPropertyName("_default")] string Default
    );

}
