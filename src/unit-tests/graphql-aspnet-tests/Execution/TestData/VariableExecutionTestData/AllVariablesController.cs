// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.VariableExecutionTestData
{
    using System;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;

    internal class AllVariablesController : GraphController
    {
        [QueryRoot]
        public sbyte ParseSbytes(sbyte regularSbyte, sbyte? nullSbyte)
        {
            return (sbyte)(regularSbyte + (nullSbyte ?? (sbyte)0));
        }

        [QueryRoot]
        public short ParseShorts(short regularShort, short? nullShort)
        {
            return (short)(regularShort + (nullShort ?? (short)0));
        }

        [QueryRoot]
        public int ParseInts(int regularInt, int? nullInt)
        {
            return regularInt + (nullInt ?? 0);
        }

        [QueryRoot]
        public long ParseLongs(long regularLong, long? nullLong)
        {
            return regularLong + (nullLong ?? 0);
        }

        [QueryRoot]
        public byte ParseBytes(byte regularByte, byte? nullByte)
        {
            return (byte)(regularByte + (nullByte ?? (byte)0));
        }

        [QueryRoot]
        public ushort ParseUshorts(ushort regularUshort, ushort? nullUshort)
        {
            return (ushort)(regularUshort + (nullUshort ?? (ushort)0));
        }

        [QueryRoot]
        public uint ParseUints(uint regularUint, uint? nullUint)
        {
            return regularUint + (nullUint ?? 0);
        }

        [QueryRoot]
        public ulong ParseUlongs(ulong regularUlong, ulong? nullUlong)
        {
            return regularUlong + (nullUlong ?? 0);
        }

        [QueryRoot]
        public float ParseFloats(float regularFloat, float? nullFloat)
        {
            return regularFloat + (nullFloat ?? 0);
        }

        [QueryRoot]
        public double ParseDoubles(double regularDouble, double? nullDouble)
        {
            return regularDouble + (nullDouble ?? 0);
        }

        [QueryRoot]
        public decimal ParseDecimals(decimal regularDecimal, decimal? nullDecimal)
        {
            return regularDecimal + (nullDecimal ?? 0);
        }

        [QueryRoot]
        public int ParseBooleans(bool regularBoolean, bool? nullBoolean)
        {
            var result = 0;
            result += regularBoolean ? 10 : 0;

            if (nullBoolean.HasValue)
            {
                result += nullBoolean.Value ? 1000 : 100;
            }

            return result;
        }

        [QueryRoot]
        public string ParseStrings(
            [FromGraphQL(TypeExpression = "Type!")]
            string regularString,
            string nullString)
        {
            return regularString + " " + (nullString ?? string.Empty);
        }

        [QueryRoot]
        public GraphId ParseIds(GraphId regularId, GraphId? nullId)
        {
            return (GraphId)(regularId + " " + (nullId ?? string.Empty));
        }

        [QueryRoot]
        public Uri ParseUris(
            [FromGraphQL(TypeExpression = "Type!")]
            Uri regularUri,
            Uri nullUri)
        {
            var uri = regularUri;
            if (nullUri != null)
            {
                uri = new Uri(uri, nullUri);
            }

            return uri;
        }

        [QueryRoot]
        public string ParseGuids(Guid regularGuid, Guid? nullGuid)
        {
            var str = regularGuid.ToString() + " ";
            if (nullGuid.HasValue)
            {
                str += nullGuid.Value.ToString();
            }

            return str;
        }

        [QueryRoot]
        public string ParseDateTimes(DateTime regularDateTime, DateTime? nullDateTime)
        {
            return regularDateTime.ToUniversalTime().ToRfc3339String() + " " + (nullDateTime.HasValue ? nullDateTime.Value.ToUniversalTime().ToRfc3339String() : string.Empty);
        }

        [QueryRoot]
        public string ParseDateTimeOffsets(DateTimeOffset regularDateTimeOffset, DateTimeOffset? nullDateTimeOffset)
        {
            return regularDateTimeOffset.ToUniversalTime().ToRfc3339String() + " " + (nullDateTimeOffset.HasValue ? nullDateTimeOffset.Value.ToUniversalTime().ToRfc3339String() : string.Empty);
        }

        [QueryRoot]
        public string ParseDateOnlys(DateOnly regularDateOnly, DateOnly? nullDateOnly)
        {
            return regularDateOnly.ToRfc3339String() + " " + (nullDateOnly.HasValue ? nullDateOnly.Value.ToRfc3339String() : string.Empty);
        }

        [QueryRoot]
        public string ParseTimeOnlys(TimeOnly regularTimeOnly, TimeOnly? nullTimeOnly)
        {
            return regularTimeOnly.ToRfc3339String() + " " + (nullTimeOnly.HasValue ? nullTimeOnly.Value.ToRfc3339String() : string.Empty);
        }

        [QueryRoot]
        public VariableSuppliedRepeatableObject ParseObject(
            [FromGraphQL(TypeExpression = "Type!")]
            VariableSuppliedRepeatableObject item,
            VariableSuppliedRepeatableObject nullItem)
        {
            return item;
        }
    }
}