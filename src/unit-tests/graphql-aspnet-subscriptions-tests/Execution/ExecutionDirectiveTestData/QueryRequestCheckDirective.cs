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

    public class QueryRequestCheckDirective : GraphDirective
    {
        public string ExpectedQueryText { get; set; }

        public bool QueryRequestReceived { get; set; }

        [DirectiveLocations(DirectiveLocation.AllExecutionLocations)]
        public IGraphActionResult Execute()
        {
            if (this.Request.QueryRequest?.QueryText == this.ExpectedQueryText)
                this.QueryRequestReceived = true;

            return this.Ok();
        }
    }
}