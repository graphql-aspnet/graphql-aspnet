// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Extensions
{
    using System;
    using System.Globalization;

    /// <summary>
    /// A collection of common <see cref="DateTime" /> extensions.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// A date format string that conforms to the the RFC3339 specification.
        /// </summary>
        public const string RFC3339_DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:ss.fffzzz";

        /// <summary>
        /// Converts the datetime into its equivilant set of milliseconds from the epoch.
        /// </summary>
        /// <param name="dateTime">The datetime to convert.</param>
        /// <returns>System.Nullable&lt;System.Int64&gt;.</returns>
        public static long? ToEpochTimeMilliseconds(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return null;

            return ToEpochTimeMilliseconds(dateTime.Value);
        }

        /// <summary>
        /// Converts the datetime into its equivilant set of milliseconds from the epoch.
        /// </summary>
        /// <param name="dateTime">The datetime to convert.</param>
        /// <returns>System.Int64.</returns>
        public static long ToEpochTimeMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(new DateTime(dateTime.Ticks, DateTimeKind.Utc)).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Converts the datetime into its equivilant set of milliseconds from the epoch.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>System.Int64.</returns>
        public static long? ToUnixTimeMilliseconds(this DateTime? dateTime)
        {
            return dateTime.ToEpochTimeMilliseconds();
        }

        /// <summary>
        /// Converts the datetime into its equivilant set of milliseconds from the epoch.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>System.Int64.</returns>
        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            return dateTime.ToEpochTimeMilliseconds();
        }

        /// <summary>
        /// Converts a numeric value (in number of milliseconds from the epoch) to a date.
        /// </summary>
        /// <param name="ticks">The ticks.</param>
        /// <returns>System.Nullable&lt;DateTime&gt;.</returns>
        public static DateTime? MillisecondsFromEpochToDateTime(long? ticks)
        {
            if (!ticks.HasValue)
                return null;

            return DateTimeOffset.FromUnixTimeMilliseconds(ticks.Value).DateTime;
        }

        /// <summary>
        /// Attempts to parse the text into a datetime using a multi format parsing strategy. Attempts a regular datetime parse as well as
        /// as a unix time stamp in seconds and milliseconds. A text value provided in ticks, when converted
        /// to a number that is less than 11 digits long is interpreted as seconds from the epoch date.
        /// Values of 12 digits or more are treated as milliseconds from the epoch date.nds.
        ///
        /// 0-11 digits treated as seconds: 1970-1-1 to 5139-11-16.
        /// 12 or more digits treated as milliseconds: 1973-03-03 09:46:39 to 33658-09-27.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="date">The output date.</param>
        /// <returns><c>true</c> if parsed successfully, <c>false</c> otherwise.</returns>
        public static bool TryParseMultiFormat(string text, out DateTime? date)
        {
            if (string.IsNullOrEmpty(text))
            {
                date = null;
                return true;
            }

            if (long.TryParse(text, out var ticks))
            {
                if (TryParseMultiFormat(ticks, out date))
                    return true;
            }

            if (DateTime.TryParse(text, out var dt))
            {
                date = dt;
                return true;
            }

            date = null;
            return false;
        }

        /// <summary>
        /// Attempts to parse the text into a datetime using a multi format parsing strategy. Attempts a regular datetime parse as well as
        /// as a unix time stamp in seconds and milliseconds. For the provided number of ticks
        /// if the number of digists is less than 11 digits long it is interpras seconds from the epoch date
        /// otherwie its interpreted as as milliseconds from the epoch date.
        ///
        /// 0-11 digits treated as seconds: 1970-1-1 to 5139-11-16.
        /// 12 or more digits treated as milliseconds: 1973-03-03 09:46:39 to 33658-09-27.
        /// </summary>
        /// <param name="ticks">The number of ticks to convert.</param>
        /// <param name="date">The output date.</param>
        /// <returns><c>true</c> if parsed successfully, <c>false</c> otherwise.</returns>
        public static bool TryParseMultiFormat(long ticks, out DateTime? date)
        {
            // boundries for datetime
            if (ticks < -62135596800000 || ticks > 253402300799999)
            {
                date = null;
                return false;
            }

            // educated guess that no time stamp (if provided in milliseconds) will be less than 12 digits
            // Resultant Assumed Boundries on Ticks are:
            // ----------------------------------------------------------------------
            // 0-11 digits treated as seconds: 1/1/1970 to 11/16/5139
            // 12 or more digits treated as milliseconds: 3/3/1973 09:46:39 to 9/27/33658 (lol)
            if (Math.Log10(ticks) + 1 >= 12)
                date = DateTimeExtensions.MillisecondsFromEpochToDateTime(ticks);
            else
                date = DateTimeOffset.FromUnixTimeSeconds(ticks).DateTime;

            return true;
        }

        /// <summary>
        /// Converts the datetime to a RFC3339 formatted string (yyyy-MM-dd'T'HH:mm:ss.fffzzz).
        /// spec: https://www.ietf.org/rfc/rfc3339.txt .
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>System.String.</returns>
        public static string ToRfc3339String(this DateTime dateTime)
        {
            return dateTime.ToString(RFC3339_DATE_FORMAT, DateTimeFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Converts the datetime to a RFC3339 formatted string (yyyy-MM-dd'T'HH:mm:ss.fffzzz).
        /// spec: https://www.ietf.org/rfc/rfc3339.txt .
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>System.String.</returns>
        public static string ToRfc3339String(this DateTimeOffset dateTime)
        {
            return dateTime.ToString(RFC3339_DATE_FORMAT, DateTimeFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Converts the datetime to a RFC3339 formatted string (yyyy-MM-dd'T'HH:mm:ss.fffzzz).
        /// spec: https://www.ietf.org/rfc/rfc3339.txt .
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>System.String.</returns>
        public static string ToRfc3339String(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return null;

            return dateTime.Value.ToString(RFC3339_DATE_FORMAT, DateTimeFormatInfo.InvariantInfo);
        }

        /// <summary>
        /// Converts the datetime to a RFC3339 formatted string (yyyy-MM-dd'T'HH:mm:ss.fffzzz).
        /// spec: https://www.ietf.org/rfc/rfc3339.txt .
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>System.String.</returns>
        public static string ToRfc3339String(this DateTimeOffset? dateTime)
        {
            if (!dateTime.HasValue)
                return null;

            return dateTime.Value.ToString(RFC3339_DATE_FORMAT, DateTimeFormatInfo.InvariantInfo);
        }
    }
}