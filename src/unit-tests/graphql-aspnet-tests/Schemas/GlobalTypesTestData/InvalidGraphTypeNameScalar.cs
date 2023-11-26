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

    public class InvalidGraphTypeNameScalar : ScalarGraphTypeBase
    {
        public InvalidGraphTypeNameScalar()
            : base("name", typeof(TwoPropertyObject))
        {
            this.Name = "Not#$aName";
            this.ValueType = ScalarValueType.String;
        }

        public override ScalarValueType ValueType { get; }

        public override object Resolve(ReadOnlySpan<char> data)
        {
            throw new NotImplementedException();
        }
    }
}