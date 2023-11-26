﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;

    public class ArgCheckExplicitValidSchemaItemDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.SCALAR)]
        public IGraphActionResult ForTypeSystem([FromGraphQL] TwoPropertyObject arg1)
        {
            return null;
        }
    }
}