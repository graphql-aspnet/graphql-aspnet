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

    public class NoParameterlessConstructorScalar : ScalarGraphTypeBase
    {
        public NoParameterlessConstructorScalar(string name)
            : base(name, typeof(TwoPropertyObject))
        {
            this.ValueType = ScalarValueType.Boolean;
        }

        public override ScalarValueType ValueType { get; }

        public override object Resolve(ReadOnlySpan<char> data)
        {
            throw new NotImplementedException();
        }
    }
}