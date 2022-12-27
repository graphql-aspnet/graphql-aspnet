// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.ExecutionDirectiveTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [GraphType("recordLocation")]
    public class DirectiveLocationRecorderDirective : GraphDirective
    {
        public DirectiveLocation RecordedLocation { get; set; } = DirectiveLocation.NONE;

        [DirectiveLocations(DirectiveLocation.AllExecutionLocations)]
        public IGraphActionResult Execute()
        {
            this.RecordedLocation = this.DirectiveLocation;
            return this.Ok();
        }
    }
}