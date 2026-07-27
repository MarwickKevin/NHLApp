using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHLApp.Application.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogInformation(this ILogger logger, string message, ConsoleColor consoleColor, params object?[] args)
        {
            // Save the current color to restore it later
            var previousColor = Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = consoleColor;

                // Call the standard logger
                logger.LogInformation("{Message}", message);
            }
            finally
            {
                // Always reset the color, even if logging throws an exception
                Console.ForegroundColor = previousColor;
            }
        }
    }
}
