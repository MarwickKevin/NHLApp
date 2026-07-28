using Microsoft.Extensions.Logging;
using NHLApp.Application.Contexts;
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
        /// Attempts to deserialize a JSON string into an object of type T. Returns false if deserialization fails. 
        /// </summary>
        public static bool TryDeserializeSafe<T>(
            this string? json,
            out T? result,
            out Exception? caughtException) where T : class
        {
            caughtException = null;
            result = default;

            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                result = JsonSerializer.Deserialize<T>(json, DefaultOptions);
                return result != null;
            }
            catch (JsonException ex)
            {
                caughtException = ex;
                return false;
            }
        }

    }
}
