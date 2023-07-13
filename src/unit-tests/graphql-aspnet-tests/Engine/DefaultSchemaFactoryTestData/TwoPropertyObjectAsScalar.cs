// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.DefaultSchemaFactoryTestData
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class TwoPropertyObjectAsScalar : ScalarGraphTypeBase
    {
        public TwoPropertyObjectAsScalar()
            : base(nameof(TwoPropertyObjectAsScalar), typeof(TwoPropertyObject))
        {
        }

        public override object Resolve(ReadOnlySpan<char> data)
        {
            return null;
        }

        public override ScalarValueType ValueType => ScalarValueType.String;
    }
}