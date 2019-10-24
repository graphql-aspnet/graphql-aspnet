// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Common
{
    using System;
    using System.Text.Json;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Json;
    using NUnit.Framework;

    [TestFixture]
    public class JsonDateTimeSerializerTests
    {
        private string Serialize(object o)
        {
            var settings = new JsonSerializerOptions();
            settings.AllowTrailingCommas = true;
            settings.Converters.Add(new JsonDateTimeOffsetRfc3339Converter());
            settings.Converters.Add(new JsonDateTimeRfc3339Converter());

            return JsonSerializer.Serialize(o, settings);
        }

        private T Deserialize<T>(string text)
        {
            var settings = new JsonSerializerOptions();
            settings.AllowTrailingCommas = true;
            settings.Converters.Add(new JsonDateTimeOffsetRfc3339Converter());
            settings.Converters.Add(new JsonDateTimeRfc3339Converter());

            return JsonSerializer.Deserialize<T>(text, settings);
        }

        [Test]
        public void Serialize_DateTime_Rfc3339DateSerializer()
        {
            var dt = DateTime.UtcNow;
            var expected = dt.ToRfc3339String();

            var result = this.Serialize(dt);

            Assert.AreEqual($"\"{expected}\"", result);
        }

        [Test]
        public void Serialize_DateTimeOffset_Rfc3339DateSerializer()
        {
            var dt = DateTimeOffset.UtcNow;
            var expected = dt.ToRfc3339String();
            var result = this.Serialize(dt);

            Assert.AreEqual($"\"{expected}\"", result);
        }

        [Test]
        public void Serialize_NullableDateTime_Rfc3339DateSerializer()
        {
            DateTime? dt = DateTime.UtcNow;
            var expected = dt.ToRfc3339String();
            var result = this.Serialize(dt);

            Assert.AreEqual($"\"{expected}\"", result);
        }

        [Test]
        public void Serialize_NullableDateTimeOffset_Rfc3339DateSerializer()
        {
            DateTimeOffset? dt = DateTimeOffset.UtcNow;
            var expected = dt.ToRfc3339String();
            var result = this.Serialize(dt);

            Assert.AreEqual($"\"{expected}\"", result);
        }

        [Test]
        public void Deserialize_DateTime_Rfc3339DateSerializer()
        {
            var expected = DateTime.UtcNow;
            var serialized = $"\"{expected.ToRfc3339String()}\"";
            var result = this.Deserialize<DateTime>(serialized);

            result = result.ToUniversalTime();

            Assert.AreEqual(expected.Year, result.Year);
            Assert.AreEqual(expected.Month, result.Month);
            Assert.AreEqual(expected.Day, result.Day);
            Assert.AreEqual(expected.Hour, result.Hour);
            Assert.AreEqual(expected.Minute, result.Minute);
            Assert.AreEqual(expected.Second, result.Second);
            Assert.AreEqual(expected.Millisecond, result.Millisecond);
        }

        [Test]
        public void Deserialize_DateTimeOffSet_Rfc3339DateSerializer()
        {
            var expected = DateTimeOffset.UtcNow;
            var serialized = $"\"{expected.ToRfc3339String()}\"";
            var result = this.Deserialize<DateTimeOffset>(serialized);

            result = result.ToUniversalTime();

            Assert.AreEqual(expected.Year, result.Year);
            Assert.AreEqual(expected.Month, result.Month);
            Assert.AreEqual(expected.Day, result.Day);
            Assert.AreEqual(expected.Hour, result.Hour);
            Assert.AreEqual(expected.Minute, result.Minute);
            Assert.AreEqual(expected.Second, result.Second);
            Assert.AreEqual(expected.Millisecond, result.Millisecond);
        }

        [Test]
        public void Deserialize_DateTime_SecondsAsNumber()
        {
            var expected = new DateTime(2019, 09, 23, 21, 55, 12);
            var serialized = $"1569275712";
            var result = this.Deserialize<DateTime>(serialized);

            Assert.AreEqual(expected.Year, result.Year);
            Assert.AreEqual(expected.Month, result.Month);
            Assert.AreEqual(expected.Day, result.Day);
            Assert.AreEqual(expected.Hour, result.Hour);
            Assert.AreEqual(expected.Minute, result.Minute);
            Assert.AreEqual(expected.Second, result.Second);
            Assert.AreEqual(expected.Millisecond, result.Millisecond);
        }

        [Test]
        public void Deserialize_DateTime_MilliSecondsAsNumber()
        {
            var expected = new DateTime(2019, 09, 23, 21, 55, 12, 15);
            var serialized = $"1569275712015";
            var result = this.Deserialize<DateTime>(serialized);

            Assert.AreEqual(expected.Year, result.Year);
            Assert.AreEqual(expected.Month, result.Month);
            Assert.AreEqual(expected.Day, result.Day);
            Assert.AreEqual(expected.Hour, result.Hour);
            Assert.AreEqual(expected.Minute, result.Minute);
            Assert.AreEqual(expected.Second, result.Second);
            Assert.AreEqual(expected.Millisecond, result.Millisecond);
        }

        [Test]
        public void Deserialize_DateTimeOffset_SecondsAsNumber()
        {
            var expected = new DateTime(2019, 09, 23, 21, 55, 12);
            var serialized = $"1569275712";
            var result = this.Deserialize<DateTimeOffset>(serialized);

            Assert.AreEqual(expected.Year, result.Year);
            Assert.AreEqual(expected.Month, result.Month);
            Assert.AreEqual(expected.Day, result.Day);
            Assert.AreEqual(expected.Hour, result.Hour);
            Assert.AreEqual(expected.Minute, result.Minute);
            Assert.AreEqual(expected.Second, result.Second);
            Assert.AreEqual(expected.Millisecond, result.Millisecond);
        }

        [Test]
        public void Deserialize_DateTimeOffset_MilliSecondsAsNumber()
        {
            var expected = new DateTime(2019, 09, 23, 21, 55, 12, 15);
            var serialized = $"1569275712015";
            var result = this.Deserialize<DateTimeOffset>(serialized);

            Assert.AreEqual(expected.Year, result.Year);
            Assert.AreEqual(expected.Month, result.Month);
            Assert.AreEqual(expected.Day, result.Day);
            Assert.AreEqual(expected.Hour, result.Hour);
            Assert.AreEqual(expected.Minute, result.Minute);
            Assert.AreEqual(expected.Second, result.Second);
            Assert.AreEqual(expected.Millisecond, result.Millisecond);
        }
    }
}