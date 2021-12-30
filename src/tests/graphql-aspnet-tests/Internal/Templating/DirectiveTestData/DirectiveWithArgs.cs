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

    public class DirectiveWithArgs : GraphDirective
    {
        public DirectiveWithArgs()
        {
        }

        [DirectiveLocations(DirectiveLocation.AllTypeSystemLocations)]
        public IGraphActionResult Execute(int arg1, string arg2)
        {
            return null;
        }
    }
}