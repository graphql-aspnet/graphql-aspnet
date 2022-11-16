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
    using GraphQL.AspNet.Common.Extensions;
    using NUnit.Framework;

    [TestFixture]
    public class DateTimeExtensionTests_TimeOnly
    {
        [Test]
        public void TimeOnly_ToRfc3339String()
        {
            // ensure the format doesnt change in the source file
            var dt = DateTime.Now;
            var timeOnly = TimeOnly.FromDateTime(dt);

            Assert.AreEqual(dt.ToString("HH:mm:ss.fff"), timeOnly.ToRfc3339String());
        }

        private static readonly object[] TIMEONLY_TRYPARSE_TESTS =
        {
            new object[] { "0", true,            new TimeOnly(0) },
            new object[] { "1", true,            new TimeOnly(1) },
            new object[] { "000000000001", true, new TimeOnly(1) },
            new object[] { null, true, null },
            new object[] { "1557878400", true,    new TimeOnly(1557878400) },
            new object[] { "12:05:05", true, new TimeOnly(12, 5, 5) },
            new object[] { "13:15:10.123", true, new TimeOnly(13, 15, 10, 123) },

            // out of bounds
            new object[] { "9223372036854775807", false, null },  // max long, greater than max tick count
            new object[] { "-1", false, null },
            new object[] { "ABC123", false, null },
        };

        [TestCaseSource(nameof(TIMEONLY_TRYPARSE_TESTS))]
        public void DateOnly_TryParseMultiFormat(string text, bool shouldParse, TimeOnly? expectedOutputUTC)
        {
            var success = DateTimeExtensions.TryParseMultiFormat(text, out TimeOnly? dt);
            Assert.AreEqual(shouldParse, success);

            Assert.AreEqual(expectedOutputUTC.HasValue, dt.HasValue);
            if (dt.HasValue && expectedOutputUTC.HasValue)
            {
                Assert.AreEqual(expectedOutputUTC.Value, dt.Value);
            }
        }
    }
}
#endif