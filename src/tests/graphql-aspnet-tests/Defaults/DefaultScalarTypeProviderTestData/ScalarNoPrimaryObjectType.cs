// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Defaults.DefaultScalarTypeProviderTestData
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class ScalarNoPrimaryObjectType : ScalarTestBase
    {
        public ScalarNoPrimaryObjectType()
        {
            this.ObjectType = null;
        }
    }
}