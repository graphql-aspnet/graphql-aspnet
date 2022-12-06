// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.DirectiveProcessorTypeSystemLocationTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class LocationTestDirective : GraphDirective
    {
        public DirectiveLocation ExecutedLocation { get; set; }

        public LocationTestDirective()
        {
            this.ExecutedLocation = DirectiveLocation.NONE;
        }

        [DirectiveLocations(DirectiveLocation.AllTypeSystemLocations)]
        public IGraphActionResult Execute()
        {
            this.ExecutedLocation = this.DirectiveLocation;
            return this.Ok();
        }
    }
}