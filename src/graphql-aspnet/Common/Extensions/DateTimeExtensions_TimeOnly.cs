// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

#if NET6_0_OR_GREATER
namespace GraphQL.AspNet.Common.Extensions
{
    using System;

    /// <summary>
    /// A collection of common Date and Time extensions.
    /// </summary>
    public partial class DateTimeExtensions
    {
        /// <summary>
        /// A <see cref="TimeOnly"/> format string that conforms to the the RFC3339 specification.
        /// </summary>
        public const string RFC3339_TIMEONLY_FORMAT = "HH:mm:ss.fff";

        /// <summary>
        /// Attempts a regular time parse (e.g. HH:mm:ss.fff). When a text value provided in ticks that number
        /// is validates as the number of 100-nanosecond units since '00:00:00.000'.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <param name="time">The output time value.</param>
        /// <returns><c>true</c> if parsed successfully, <c>false</c> otherwise.</returns>
        public static bool TryParseMultiFormat(string text, out TimeOnly? time)
        {
            time = null;

            if (string.IsNullOrWhiteSpace(text))
                return true;

            var success = false;
            if (TimeOnly.TryParse(text, out var timeOut))
            {
                time = timeOut;
                success = true;
            }
            else if (long.TryParse(text, out var ticks))
            {
                if (ticks >= 0 && ticks <= TimeOnly.MaxValue.Ticks)
                {
                    time = new TimeOnly(ticks);
                    success = true;
                }
            }

            return success;
        }

        /// <summary>
        /// Converts the time to a RFC3339 formatted date string (e.g. hh:mm:ss.fff).
        /// spec: https://www.ietf.org/rfc/rfc3339.txt .
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns>System.String.</returns>
        public static string ToRfc3339String(this TimeOnly time)
        {
            return time.ToString(RFC3339_TIMEONLY_FORMAT);
        }

        /// <summary>
        /// Converts the time to a RFC3339 formatted date string (e.g. hh:mm:ss.fff).
        /// spec: https://www.ietf.org/rfc/rfc3339.txt .
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns>System.String.</returns>
        public static string ToRfc3339String(this TimeOnly? time)
        {
            if (!time.HasValue)
                return null;

            return time.Value.ToRfc3339String();
        }
    }
}
#endif