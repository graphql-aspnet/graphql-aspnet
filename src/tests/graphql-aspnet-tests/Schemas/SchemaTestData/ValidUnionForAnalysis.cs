// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.SchemaTestData
{
    using System;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class ValidUnionForAnalysis : GraphUnionProxy
    {
        public ValidUnionForAnalysis()
            : base(typeof(AddressData), typeof(CountryData))
        {
        }

        public override Type ResolveType(Type runtimeObjectType)
        {
            return typeof(AddressData);
        }
    }
}