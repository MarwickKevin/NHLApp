using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;

namespace NHLApp.Application.Extensions
{
    public static class LoggerExtensions
    {

    #region LOG INFORMATION WITH COLOR

        /// <summary>
        /// Logs an information message with a specified console color
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="consoleColor"></param>
        /// <param name="args"></param>
        public static void LogInformationWithColor(this ILogger logger, string message, ConsoleColor consoleColor, params object?[] args)
        {
            // Log the message using the logger first
            if (args.Length > 0)
            {
                logger.LogInformation(message, args);
            }
            else
            {
                logger.LogInformation("{Message}", message);
            }

            // Then, write the message to the console with the specified color
            var previousColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = consoleColor;

                string formattedMessage = FormatMessage(message, args);

                Console.WriteLine(formattedMessage);
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

    #endregion

    #region LOG ERRORS WITH COLOR

        /// <summary>
        /// Logs an error message with a specified console color. Exception details are included in the console output.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        /// <param name="consoleColor"></param>
        /// <param name="args"></param>
        public static void LogErrorWithColor(this ILogger logger, Exception ex, string message, ConsoleColor consoleColor, params object?[] args)
        {
            // Log the message using the logger first
            if (args.Length > 0)
            {
                logger.LogError(ex, message, args);
            }
            else
            {
                logger.LogError(ex, "{Message}", message);
            }

            // Then, write the message to the console with the specified color
            var previousColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = consoleColor;
                string formattedMessage = FormatMessage(message, args);
                Console.WriteLine($"{formattedMessage}\n{ex}");
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

        /// <summary>
        /// Logs an error message with a specified console color, without exception details.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        /// <param name="consoleColor"></param>
        /// <param name="args"></param>
        public static void LogErrorWithColor(this ILogger logger, string message, ConsoleColor consoleColor, params object?[] args)
        {
            // Log the message using the logger first
            if (args.Length > 0)
            {
                logger.LogError(message, args);
            }
            else
            {
                logger.LogError("{Message}", message);
            }

            // Then, write the message to the console with the specified color
            var previousColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = consoleColor;

                string formattedMessage = FormatMessage(message, args);

                Console.WriteLine(formattedMessage);
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

    #endregion

    #region PRIVATE HELPER

        private static string FormatMessage(string message, object?[] args)
        {
            if (args == null || args.Length == 0) return message;

            int argIndex = 0;
            
            return Regex.Replace(message, @"\{[a-zA-Z0-9_]+(?::[^}]*)?\}", match =>
            {
            
                if (argIndex < args.Length)
                {
                    string valueStr = args[argIndex]?.ToString() ?? "null";
                    argIndex++;
                    return valueStr;
                }

                return match.Value;
            }, RegexOptions.None, TimeSpan.FromMilliseconds(100));
        }

    #endregion

    }
}