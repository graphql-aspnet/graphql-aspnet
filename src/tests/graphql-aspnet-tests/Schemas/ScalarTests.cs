// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas
{
    using System;
    using System.Reflection;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using NUnit.Framework;

    [TestFixture]
    public class ScalarTests
    {
        private static readonly object[] SCALAR_RESOLVER_TEST_DATA =
        {
            new object[] { typeof(BooleanScalarType), "true", true, false },
            new object[] { typeof(BooleanScalarType), "false", false, false },
            new object[] { typeof(BooleanScalarType), "\"bob\"", null, true },

            new object[] { typeof(ByteScalarType), "1", (byte)1, false },
            new object[] { typeof(ByteScalarType), "0", (byte)0, false },
            new object[] { typeof(ByteScalarType), "\"1\"", null, true },
            new object[] { typeof(ByteScalarType), "100000000000", null, true },
            new object[] { typeof(ByteScalarType), "\"bob\"", null, true },

            new object[] { typeof(DateTimeScalarType), "\"2021-11-12\"", new DateTime(2021, 11, 12, 0, 0, 0, DateTimeKind.Utc), false },
            new object[] { typeof(DateTimeScalarType), "\"2021-10-21T11:12:13+00:00\"", new DateTime(2021, 10, 21, 11, 12, 13, DateTimeKind.Utc), false },
            new object[] { typeof(DateTimeScalarType), "0", new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), false },

            new object[] { typeof(DateTimeScalarType), "1", new DateTime(1970, 1, 1, 0, 0, 1, 0, DateTimeKind.Utc), false },
            new object[] { typeof(DateTimeScalarType), "1557950810035", new DateTime(2019, 5, 15, 20, 6, 50, 35, DateTimeKind.Utc), false },

            new object[] { typeof(DateTimeOffsetScalarType), "1", new DateTimeOffset(1970, 1, 1, 0, 0, 1, 0, TimeSpan.Zero), false },
            new object[] { typeof(DateTimeOffsetScalarType), "\"2021-10-21T11:12:13+00:00\"", new DateTimeOffset(2021, 10, 21, 11, 12, 13, TimeSpan.Zero), false },
            new object[] { typeof(DateTimeOffsetScalarType), "1557950810035", new DateTimeOffset(2019, 5, 15, 20, 6, 50, 35, TimeSpan.Zero), false },
            new object[] { typeof(DateTimeOffsetScalarType), "\"abc\"", null, true },

            new object[] { typeof(DecimalScalarType), "1.234", 1.234m, false },
            new object[] { typeof(DecimalScalarType), "\"1.234\"", null, true },
            new object[] { typeof(DecimalScalarType), "\"abc\"", null, true },

            new object[] { typeof(DoubleScalarType), "1.234", 1.234d, false },
            new object[] { typeof(DoubleScalarType), "\"1.234\"", null, true },
            new object[] { typeof(DoubleScalarType), "\"abc\"", null, true },

            new object[] { typeof(FloatScalarType), "1.234", 1.234f, false },
            new object[] { typeof(FloatScalarType), "\"abc\"", null, true },

            new object[] { typeof(GraphIdScalarType), "\"abc123\"", new GraphId("abc123"), false },
            new object[] { typeof(GraphIdScalarType), "\"12345\"", new GraphId("12345"), false },
            new object[] { typeof(GraphIdScalarType), "\"\"", new GraphId(string.Empty), false },
            new object[] { typeof(GraphIdScalarType), string.Empty, null, true },
            new object[] { typeof(GraphIdScalarType), null, null, true },

            new object[] { typeof(GuidScalarType), "\"B676E2CA-BB09-4849-A102-6D9373CE9B85\"", Guid.Parse("B676E2CA-BB09-4849-A102-6D9373CE9B85"), false },
            new object[] { typeof(GuidScalarType), "12345", null, true },
            new object[] { typeof(GuidScalarType), "\"\"", null, true },

            new object[] { typeof(IntScalarType), "1", 1, false },
            new object[] { typeof(IntScalarType), "-1", -1, false },
            new object[] { typeof(IntScalarType), "\"1\"", null, true },
            new object[] { typeof(IntScalarType), "\"abc\"", null, true },

            new object[] { typeof(LongScalarType), "1", 1L, false },
            new object[] { typeof(LongScalarType), "-1", -1L, false },
            new object[] { typeof(LongScalarType), "\"1\"", null, true },
            new object[] { typeof(LongScalarType), "\"abc\"", null, true },

            new object[] { typeof(SByteScalarType), "1", (sbyte)1, false },
            new object[] { typeof(SByteScalarType), "0", (sbyte)0, false },
            new object[] { typeof(SByteScalarType), "\"1\"", null, true },
            new object[] { typeof(SByteScalarType), "100000000000", null, true },
            new object[] { typeof(SByteScalarType), "\"bob\"", null, true },

            new object[] { typeof(StringScalarType), "\"bob\"", "bob", false },
            new object[] { typeof(StringScalarType), "\"\"", string.Empty, false },
            new object[] { typeof(StringScalarType), string.Empty, null, true },
            new object[] { typeof(StringScalarType), "12345", null, true },

            new object[] { typeof(ULongScalarType), "1", 1U, false },
            new object[] { typeof(ULongScalarType), "-1", null, true },
            new object[] { typeof(ULongScalarType), "\"1\"", null, true },
            new object[] { typeof(ULongScalarType), "\"abc\"", null, true },

            new object[] { typeof(UriScalarType), "\"http://unknownwebsite.com\"", new Uri("http://unknownwebsite.com", UriKind.RelativeOrAbsolute), false },
            new object[] { typeof(UriScalarType), "-1", null, true },
            new object[] { typeof(UriScalarType), "\"abc\"", new Uri("abc", UriKind.RelativeOrAbsolute), false },

#if NET6_0_OR_GREATER
            new object[] { typeof(DateOnlyScalarType), "\"2021-11-12\"", new DateOnly(2021, 11, 12), false },
            new object[] { typeof(DateOnlyScalarType), "\"2021-10-21T11:12:13+00:00\"", new DateOnly(2021, 10, 21), false },
            new object[] { typeof(DateOnlyScalarType), "0", new DateOnly(1970, 1, 1), false },
            new object[] { typeof(DateOnlyScalarType), "\"abc\"", null, true },

            new object[] { typeof(TimeOnlyScalarType), "\"10:11:12.789\"", new TimeOnly(10, 11, 12, 789), false },
            new object[] { typeof(TimeOnlyScalarType), "0", new TimeOnly(0), false },
            new object[] { typeof(TimeOnlyScalarType), "1", new TimeOnly(1), false },
            new object[] { typeof(TimeOnlyScalarType), "-1", null, true },
            new object[] { typeof(TimeOnlyScalarType), "\"2021-10-21T11:12:13+00:00\"", null, true },
            new object[] { typeof(TimeOnlyScalarType), "\"abc\"", null, true },
#endif
        };

        [TestCaseSource(nameof(SCALAR_RESOLVER_TEST_DATA))]
        public void ScalarResolver(Type scalarType, string testValue, object expectedResolvedValue, bool shouldThrow)
        {
            var prop = scalarType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var instance = prop.GetValue(null, null) as BaseScalarType;

            if (shouldThrow)
            {
                Assert.Throws<UnresolvedValueException>(() => instance.Resolve(testValue.AsSpan()));
            }
            else
            {
                var data = instance.Resolve(testValue.AsSpan());

                if (data is DateTime dt)
                    data = dt.ToUniversalTime();
                if (data is DateTimeOffset dto)
                    data = dto.ToUniversalTime();

                Assert.AreEqual(expectedResolvedValue, data);
            }
        }

        private static readonly object[] SCALAR_SERIALIZER_TEST_DATA =
        {
            new object[] { typeof(BooleanScalarType), true, true },
            new object[] { typeof(BooleanScalarType), false, false },

            new object[] { typeof(ByteScalarType), (byte)1, (byte)1 },
            new object[] { typeof(DateTimeScalarType), DateTime.Parse("2021-10-11 05:00AM"), DateTime.Parse("2021-10-11 05:00AM") },
            new object[] { typeof(DateTimeOffsetScalarType), DateTimeOffset.Parse("2021-10-11 05:00AM"), DateTimeOffset.Parse("2021-10-11 05:00AM") },

            new object[] { typeof(DecimalScalarType), 1.234m, 1.234m },
            new object[] { typeof(DoubleScalarType), 1.234d, 1.234d },
            new object[] { typeof(FloatScalarType), 1.234f, 1.234f },
            new object[] { typeof(GraphIdScalarType), new GraphId("abc123"), "abc123" },
            new object[] { typeof(GraphIdScalarType), null, null },
            new object[] { typeof(GuidScalarType), Guid.Parse("B676E2CA-BB09-4849-A102-6D9373CE9B85"), "b676e2ca-bb09-4849-a102-6d9373ce9b85" },
            new object[] { typeof(GuidScalarType), null, null },
            new object[] { typeof(IntScalarType), 1, 1, },
            new object[] { typeof(LongScalarType), 15L, 15L },
            new object[] { typeof(SByteScalarType), (sbyte)1, (sbyte)1 },
            new object[] { typeof(StringScalarType), "abc123", "abc123" },
            new object[] { typeof(StringScalarType), null, null },

            new object[] { typeof(UIntScalarType), 1U, 1U, },
            new object[] { typeof(ULongScalarType), 15UL, 15UL },

            new object[] { typeof(UriScalarType), new Uri("http://fakewebsite.com", UriKind.RelativeOrAbsolute), "http://fakewebsite.com/" },
            new object[] { typeof(UriScalarType), null, null },

#if NET6_0_OR_GREATER
            new object[] { typeof(DateOnlyScalarType), new DateOnly(2021, 10, 11), new DateOnly(2021, 10, 11) },
            new object[] { typeof(TimeOnlyScalarType), new TimeOnly(10, 11, 12), new TimeOnly(10, 11, 12) },
#endif
        };

        [TestCaseSource(nameof(SCALAR_SERIALIZER_TEST_DATA))]
        public void ScalarSerializer(Type scalarType, object validTestValue, object expectedOutput)
        {
            var prop = scalarType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var instance = prop.GetValue(null, null) as BaseScalarType;

            var data = instance.Serialize(validTestValue);
            Assert.AreEqual(expectedOutput, data);
        }
    }
}