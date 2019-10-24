// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

// ReSharper disable All
namespace GraphQL.AspNet.Tests.CommonHelpers
{
    /// <summary>
    /// An object representing a complex data object with properties that are a mix of scalars and objects.
    /// </summary>
    public class CompletePropertyObject
    {
        public string StringProperty { get; set; }

        public TwoPropertyObject ObjectProperty { get; set; }

        public TwoPropertyObjectV2 ObjectPropertyV2 { get; set; }

        public decimal? NullableDecimalProperty { get; set; }

        public int IntProperty { get; set; }

        public long LongProperty { get; set; }

        public ulong? NullableULongProperty { get; set; }

        // no getter should not be part of graph
        public uint? NullableUIntPropertyNoGetter
        {
            set { }
        }
    }
}