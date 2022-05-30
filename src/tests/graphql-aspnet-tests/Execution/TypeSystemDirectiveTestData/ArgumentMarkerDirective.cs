// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTests
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class ArgumentMarkerDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.ARGUMENT_DEFINITION)]
        public IGraphActionResult PerformOperationon()
        {
            // dont actually do anything, this is just a test for inclusion
            return this.Ok();
        }
    }
}