﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Defaults.DefaultScalarTypeProviderTestData
{
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class ScalarImplementationTypeInUse : ScalarTestBase
    {
        public ScalarImplementationTypeInUse()
        {
            this.ObjectType = typeof(int);
        }
    }
}