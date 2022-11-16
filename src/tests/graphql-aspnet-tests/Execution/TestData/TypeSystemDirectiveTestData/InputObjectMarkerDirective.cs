// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.TypeSystemDirectiveTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class InputObjectMarkerDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.INPUT_OBJECT)]
        public IGraphActionResult PerformOperationon()
        {
            // dont actually do anything, this is just a test for inclusion
            return this.Ok();
        }
    }
}