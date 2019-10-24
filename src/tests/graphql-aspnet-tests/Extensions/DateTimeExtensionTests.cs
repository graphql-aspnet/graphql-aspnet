// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Extensions
{
    using System;
    using System.Globalization;
    using GraphQL.AspNet.Common.Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class DateTimeExtensionTests
    {
        private static readonly object[] DATE_TOMILLISECONDS_TESTS =
        {
            // epoch
            new object[] { new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), 0L },
            new object[] { new DateTime(1970, 1, 1, 0, 0, 0, 1, DateTimeKind.Utc), 1L },

            // nullable date time
            new object[] { null, null },

            //// as milliseconds
            new object[] { new DateTime(1997, 4, 25, 0, 0, 0, DateTimeKind.Utc), 861926400000L },
            new object[] { new DateTime(2019, 5, 15, 20, 6, 50, DateTimeKind.Utc), 1557950810000L },
            new object[] { new DateTime(2019, 5, 15, 20, 6, 50, 35, DateTimeKind.Utc), 1557950810035L },
        };

        [TestCaseSource(nameof(DATE_TOMILLISECONDS_TESTS))]
        public void DateTime_ToEpochTimeMilliseconds(DateTime? dt, long? expectedOutput)
        {
            var num = dt.ToEpochTimeMilliseconds();
            Assert.AreEqual(expectedOutput.HasValue, num.HasValue);
            if (num.HasValue && expectedOutput.HasValue)
            {
                // convert back to string when checking. due to rounding errors on date comparrisons
                Assert.AreEqual(
                    expectedOutput.Value,
                    num.Value);
            }
        }

        [TestCaseSource(nameof(DATE_TOMILLISECONDS_TESTS))]
        public void DateTime_ToUnixTimeMilliseconds(DateTime? dt, long? expectedOutput)
        {
            var num = dt.ToUnixTimeMilliseconds();
            Assert.AreEqual(expectedOutput.HasValue, num.HasValue);
            if (num.HasValue && expectedOutput.HasValue)
            {
                // convert back to string when checking. due to rounding errors on date comparrisons
                Assert.AreEqual(
                    expectedOutput.Value,
                    num.Value);
            }
        }

        private static readonly object[] DATE_MILLISECONDSTODATE_TESTS =
        {
            // epoch
            new object[] { 0L, new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new object[] { 1L, new DateTime(1970, 1, 1, 0, 0, 0, 1, DateTimeKind.Utc) },

            // nullable date time
            new object[] { null, null },

            //// as milliseconds
            new object[] { 861926400000L, new DateTime(1997, 4, 25, 0, 0, 0, DateTimeKind.Utc) },
            new object[] { 1557950810000L, new DateTime(2019, 5, 15, 20, 6, 50, DateTimeKind.Utc) },
            new object[] { 1557950810035L, new DateTime(2019, 5, 15, 20, 6, 50, 35, DateTimeKind.Utc) },
        };

        [TestCaseSource(nameof(DATE_MILLISECONDSTODATE_TESTS))]
        public void DateTime_ToDateTime(long? num, DateTime? expectedOutputUTC)
        {
            var dt = DateTimeExtensions.MillisecondsFromEpochToDateTime(num);
            Assert.AreEqual(expectedOutputUTC.HasValue, dt.HasValue);
            if (dt.HasValue && expectedOutputUTC.HasValue)
            {
                // convert back to string when checking. due to rounding errors on date comparrisons
                Assert.AreEqual(
                    expectedOutputUTC.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    dt.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }
        }

        private static readonly object[] DATE_TRYPARSE_TESTS =
        {
            // epoch
            new object[] { "0", true,            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // less than 12 digits are interpreted as "seconds"
            new object[] { "1", true,            new DateTime(1970, 1, 1, 0, 0, 1, DateTimeKind.Utc) },

            // 12 or more digits are interpreted as "milliseconds"
            new object[] { "000000000001", true, new DateTime(1970, 1, 1, 0, 0, 1, DateTimeKind.Utc) },

            // nullable date time
            new object[] { null, true, null },

            //// as seconds
            new object[] { "861926400", true,    new DateTime(1997, 4, 25, 0, 0, 0, DateTimeKind.Utc) },
            new object[] { "1557950810", true,    new DateTime(2019, 5, 15, 20, 06, 50, DateTimeKind.Utc) },

            //// as milliseconds
            new object[] { "861926400000", true, new DateTime(1997, 4, 25, 0, 0, 0, DateTimeKind.Utc) },
            new object[] { "1557950810000", true, new DateTime(2019, 5, 15, 20, 6, 50, DateTimeKind.Utc) },
            new object[] { "1557950810035", true, new DateTime(2019, 5, 15, 20, 6, 50, 35, DateTimeKind.Utc) },

            // out of bounds
            new object[] { "9223372036854775807", false, null }, // max long
            new object[] { "9223372036854775808", false, null }, // min long

            // text dates
            new object[] { "2019-05-05", true, new DateTime(2019, 5, 5, 0, 0, 0, DateTimeKind.Unspecified), },
            new object[] { "2019-06-07 8:42:05 PM", true, new DateTime(2019, 6, 7, 20, 42, 05, DateTimeKind.Unspecified), },
            new object[] { "2006-08-22T06:30:07", true, new DateTime(2006, 8, 22, 6, 30, 07, DateTimeKind.Utc), },
        };

        [TestCaseSource(nameof(DATE_TRYPARSE_TESTS))]
        public void DateTime_TryParseMultiFormat(string text, bool shouldParse, DateTime? expectedOutputUTC)
        {
            var success = DateTimeExtensions.TryParseMultiFormat(text, out var dt);
            Assert.AreEqual(shouldParse, success);

            Assert.AreEqual(expectedOutputUTC.HasValue, dt.HasValue);
            if (dt.HasValue && expectedOutputUTC.HasValue)
            {
                // convert back to string when checking. due to rounding errors on date comparrisons
                Assert.AreEqual(
                    expectedOutputUTC.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    dt.Value.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            }
        }

        [Test]
        public void ToUnixTimeMilliseconds()
        {
            // just ensure the overload is correct to the root
            var dt = DateTime.Now;

            Assert.AreEqual(dt.ToEpochTimeMilliseconds(), dt.ToUnixTimeMilliseconds());
        }

        [Test]
        public void Date_ToRfc3339String()
        {
            // ensure the format doesnt change in the source file
            var dt = DateTime.Now;
            Assert.AreEqual(dt.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo), dt.ToRfc3339String());
        }

        [Test]
        public void DateOffSet_ToRfc3339String()
        {
            // ensure the format doesnt change in the source file
            var dt = DateTimeOffset.Now;
            Assert.AreEqual(dt.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz", DateTimeFormatInfo.InvariantInfo), dt.ToRfc3339String());
        }
    }
}