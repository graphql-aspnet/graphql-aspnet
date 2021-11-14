// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

#if NET6_0_OR_GREATER
namespace GraphQL.AspNet.Tests.Extensions
{
    using System;
    using System.Globalization;
    using GraphQL.AspNet.Common.Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class DateTimeExtensionTests_DateOnly
    {
        private static readonly object[] DATEONLY_TOMILLISECONDS_TESTS =
        {
            // epoch
            new object[] { new DateOnly(1970, 1, 1), 0L },
            new object[] { new DateOnly(1970, 1, 2), 86400000L },

            // nullable date only
            new object[] { null, null },

            //// as milliseconds
            new object[] { new DateOnly(1997, 4, 25), 861926400000L },
            new object[] { new DateOnly(2019, 5, 15), 1557878400000L },
        };

        [TestCaseSource(nameof(DATEONLY_TOMILLISECONDS_TESTS))]
        public void DateOnly_ToEpochTimeMilliseconds(DateOnly? dt, long? expectedOutput)
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

        [TestCaseSource(nameof(DATEONLY_TOMILLISECONDS_TESTS))]
        public void DateOnly_ToUnixTimeMilliseconds(DateOnly? dt, long? expectedOutput)
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

        private static readonly object[] DATEONLY_MILLISECONDSTODATE_TESTS =
        {
            // epoch
            new object[] { 0L, new DateOnly(1970, 1, 1) },

            // nullable date time
            new object[] { null, null },

            //// as milliseconds
            new object[] { 861926400000L, new DateOnly(1997, 4, 25) },
            new object[] { 1557878400000L, new DateOnly(2019, 5, 15) },
        };

        [TestCaseSource(nameof(DATEONLY_MILLISECONDSTODATE_TESTS))]
        public void MillisecondsFromEpochToDateOnly(long? num, DateOnly? expectedOutput)
        {
            var dt = DateTimeExtensions.MillisecondsFromEpochToDateOnly(num);
            Assert.AreEqual(expectedOutput.HasValue, dt.HasValue);
            if (dt.HasValue && expectedOutput.HasValue)
            {
                Assert.AreEqual(expectedOutput.Value, dt.Value);
            }
        }

        private static readonly object[] DATEONLY_TRYPARSE_TESTS =
        {
            // epoch
            new object[] { "0", true,            new DateOnly(1970, 1, 1) },

            // less than 12 digits are interpreted as "seconds"
            new object[] { "1", true,            new DateOnly(1970, 1, 1) },

            // 12 or more digits are interpreted as "milliseconds"
            new object[] { "000000000001", true, new DateOnly(1970, 1, 1) },

            // nullable date time
            new object[] { null, true, null },

            //// as seconds
            new object[] { "861926400", true,    new DateOnly(1997, 4, 25) },
            new object[] { "1557878400", true,    new DateOnly(2019, 5, 15) },

            //// as milliseconds
            new object[] { "861926400000", true, new DateOnly(1997, 4, 25) },
            new object[] { "1557878400000", true, new DateOnly(2019, 5, 15) },
            new object[] { "1557889501000", true, new DateOnly(2019, 5, 15) }, // 2019-05-15 03:05 AM UTC-0

            // out of bounds
            new object[] { "9223372036854775807", false, null }, // max long
            new object[] { "9223372036854775808", false, null }, // max long + 1

            // text dates
            new object[] { "2019-05-05", true, new DateOnly(2019, 5, 5), },
            new object[] { "2019-06-07 8:42:05 PM", true, new DateOnly(2019, 6, 7), },
            new object[] { "2006-08-22T06:30:07", true, new DateOnly(2006, 8, 22), },
        };

        [TestCaseSource(nameof(DATEONLY_TRYPARSE_TESTS))]
        public void DateOnly_TryParseMultiFormat(string text, bool shouldParse, DateOnly? expectedOutputUTC)
        {
            var success = DateTimeExtensions.TryParseMultiFormat(text, out DateOnly? dt);
            Assert.AreEqual(shouldParse, success);

            Assert.AreEqual(expectedOutputUTC.HasValue, dt.HasValue);
            if (dt.HasValue && expectedOutputUTC.HasValue)
            {
                Assert.AreEqual(expectedOutputUTC.Value, dt.Value);
            }
        }

        [Test]
        public void ToUnixTimeMilliseconds()
        {
            // just ensure the overload is correct to the root
            var dt = DateOnly.FromDateTime(DateTime.Now);
            Assert.AreEqual(dt.ToEpochTimeMilliseconds(), dt.ToUnixTimeMilliseconds());
        }

        [Test]
        public void DateOnly_ToRfc3339String()
        {
            // ensure the format doesnt change in the source file
            var dt = DateTime.UtcNow;
            dt = dt.Subtract(dt.TimeOfDay);
            var dateOnly = DateOnly.FromDateTime(dt);

            Assert.AreEqual(dt.ToString("yyyy-MM-dd"), dateOnly.ToRfc3339String());
        }
    }
}

#endif