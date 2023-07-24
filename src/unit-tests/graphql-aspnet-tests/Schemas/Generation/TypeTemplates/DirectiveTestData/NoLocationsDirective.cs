﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.DirectiveTestData
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class NoLocationsDirective : GraphDirective
    {
        public Task<IGraphActionResult> Execute(int arg1, string arg2)
        {
            return null;
        }
    }
}