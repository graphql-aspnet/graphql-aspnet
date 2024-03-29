﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class DirectiveWithRequirements : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.FIELD)]
        public IGraphActionResult Execute(TwoPropertyObject obj)
        {
            return null;
        }
    }
}