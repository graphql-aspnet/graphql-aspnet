// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTestData
{
    using System;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    [ApplyDirective(typeof(ScalarMarkerDirective))]
    public class MarkedScalarByAttribute : BaseScalarType
    {
        public MarkedScalarByAttribute()
            : base("CustomScalar", typeof(MarkedScalarType))
        {
        }

        public override object Resolve(ReadOnlySpan<char> data)
        {
            return null;
        }

        public override object Serialize(object item)
        {
            return null;
        }

        public override ScalarValueType ValueType => ScalarValueType.StringOrNumber;

        public override TypeCollection OtherKnownTypes => TypeCollection.Empty;
    }
}