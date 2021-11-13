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
    public static partial class DateTimeExtensions
    {
        /// <summary>
        /// A <see cref="DateOnly"/> format string that conforms to the the RFC3339 specification.
        /// </summary>
        public const string RFC3339_DATEONLY_FORMAT = "yyyy-MM-dd";

        /// <summary>
        /// Converts the date into its equivilant set of milliseconds from the epoch in UTC-0.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>System.Nullable&lt;System.Int64&gt;.</returns>
        public static long? ToEpochTimeMilliseconds(this DateOnly? date)
        {
            if (!date.HasValue)
                return null;

            return ToEpochTimeMilliseconds(date.Value);
        }

        /// <summary>
        /// Converts the date into its equivilant set of milliseconds from the epoch in UTC-0.
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>System.Int64.</returns>
        public static long ToEpochTimeMilliseconds(this DateOnly date)
        {
            return new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Converts the date into its equivilant set of milliseconds from the epoch.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>System.Int64.</returns>
        public static long? ToUnixTimeMilliseconds(this DateOnly? date)
        {
            if (!date.HasValue)
                return null;

            return date.Value.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Converts the datetime into its equivilant set of milliseconds from the epoch.
        /// </summary>
        /// <param name="date">The date time.</param>
        /// <returns>System.Int64.</returns>
        public static long ToUnixTimeMilliseconds(this DateOnly date)
        {
            return date.ToEpochTimeMilliseconds();
        }

        /// <summary>
        /// Converts a numeric value (in number of milliseconds from the epoch) to a date. Any time component is dropped.
        /// </summary>
        /// <param name="ticks">The ticks.</param>
        /// <returns>System.Nullable&lt;DateTime&gt;.</returns>
        public static DateOnly? MillisecondsFromEpochToDateOnly(long? ticks)
        {
            var result = DateTimeExtensions.MillisecondsFromEpochToDateTime(ticks);
            if (!result.HasValue)
                return null;

            return DateOnly.FromDateTime(result.Value);
        }

        /// <summary>
        /// Attempts to parse the number of ticks into a DateOnly value using a multi format parsing strategy.
        /// The ticks provided will first be converted into a datetime from the epoch in (UTC-0),
        /// then the date component extracted.
        ///
        /// 0-11 digits treated as seconds: 1970-1-1 to 5139-11-16.
        /// 12 or more digits treated as milliseconds: 1973-03-03 09:46:39 to 33658-09-27.
        /// </summary>
        /// <param name="ticks">The number of ticks to convert.</param>
        /// <param name="date">The output date.</param>
        /// <returns><c>true</c> if parsed successfully, <c>false</c> otherwise.</returns>
        public static bool TryParseMultiFormat(long ticks, out DateOnly? date)
        {
            date = null;
            var success = DateTimeExtensions.TryParseMultiFormat(ticks, out DateTime? dt);

            if (success && dt.HasValue)
                date = DateOnly.FromDateTime(dt.Value);

            return success;
        }

        /// <summary>
        /// <para>
        /// Attempts to parse the text into a DateOnly value using a multi format parsing strategy.
        /// Attempts a regular date parse as well as as a unix time stamp in seconds and milliseconds.
        /// A text value provided in ticks, when converted
        /// to a number that is less than 11 digits long is interpreted as seconds from the epoch date.
        /// Values of 12 digits or more are treated as milliseconds from the epoch date.nds.
        /// </para>
        /// <para>
        /// 0-11 digits treated as seconds: 1970-1-1 to 5139-11-16.<br/>
        /// 12 or more digits treated as milliseconds: 1973-03-03 to 33658-09-27.
        /// </para>
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <param name="date">The output date value.</param>
        /// <returns><c>true</c> if parsed successfully, <c>false</c> otherwise.</returns>
        public static bool TryParseMultiFormat(string text, out DateOnly? date)
        {
            date = null;
            var success = DateTimeExtensions.TryParseMultiFormat(text, out DateTime? dt);

            if (success && dt.HasValue)
                date = DateOnly.FromDateTime(dt.Value);

            return success;
        }

        /// <summary>
        /// Converts the date to a RFC3339 formatted date string  (e.g. yyyy-MM-dd).
        /// spec: https://www.ietf.org/rfc/rfc3339.txt .
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>System.String.</returns>
        public static string ToRfc3339String(this DateOnly date)
        {
            return date.ToString(RFC3339_DATEONLY_FORMAT);
        }

        /// <summary>
        /// Converts the date to a RFC3339 formatted date string (e.g. yyyy-MM-dd).
        /// spec: https://www.ietf.org/rfc/rfc3339.txt .
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>System.String.</returns>
        public static string ToRfc3339String(this DateOnly? date)
        {
            if (!date.HasValue)
                return null;

            return date.Value.ToRfc3339String();
        }
    }
}
#endif