// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.GlobalTypesTestData
{
    using System;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class NoNameOnScalar : ScalarGraphTypeBase
    {
        public NoNameOnScalar()
            : base("something", typeof(TwoPropertyObject))
        {
            this.Name = null;
            this.ValueType = ScalarValueType.String;
        }

        public override ScalarValueType ValueType { get; }

        public override object Resolve(ReadOnlySpan<char> data)
        {
            throw new NotImplementedException();
        }
    }
}