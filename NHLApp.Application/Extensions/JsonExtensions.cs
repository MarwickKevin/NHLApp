using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NHLApp.Application.Extensions
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions DefaultOptions = new() 
        { 
            PropertyNameCaseInsensitive = true 
        };

        /// <summary>
        /// Attempts to deserialize a JSON string into an object of type T. If deserialization fails, logs the error and returns false.
        /// </summary>
        public static bool TryDeserializeSafe<T>(
            this string? json,
            ILogger logger,
            out T? result,
            string contextDescription,
            out Exception? caughtException,
            string? customErrorMessage = null) where T : class
        {
            caughtException = null;
            result = default;

            if (string.IsNullOrWhiteSpace(json))
            {
                logger.LogError("Cannot deserialize {ContextDescription} because the JSON string is null or empty.", contextDescription);
                return false;
            }

            try
            {
                result = JsonSerializer.Deserialize<T>(json, DefaultOptions);

                if (result == null)
                {
                    logger.LogError(customErrorMessage ?? "Deserialized result for {ContextDescription} resulted in a null payload.", contextDescription);
                    return false;
                }

                return true;
            }
            catch (JsonException ex)
            {
                var message = customErrorMessage ?? "Failed to deserialize JSON for {ContextDescription}.";
                logger.LogError(ex, message, contextDescription);

                caughtException = ex;
                return false;
            }
        }

    }
}
