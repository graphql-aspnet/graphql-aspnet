// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    public class SimpleScalarStructGraphType : ScalarGraphTypeBase
    {
        public SimpleScalarStructGraphType()
            : base(nameof(SimpleScalarStruct), typeof(SimpleScalarStruct))
        {
            this.Description = "Scalar from a struct";
        }

        public override ScalarValueType ValueType => ScalarValueType.String;

        public override object Resolve(ReadOnlySpan<char> data)
        {
            return null;
        }

        public override object Serialize(object item)
        {
            return null;
        }
    }
}