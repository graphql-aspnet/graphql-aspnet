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

    public class InvalidUnionForAnalysis : GraphUnionProxy
    {
        public InvalidUnionForAnalysis()
            : base(typeof(AddressData), typeof(CountryData))
        {
        }

        public override Type ResolveType(Type runtimeObjectType)
        {
            // returns a type not in the list
            return typeof(JobData);
        }
    }
}