// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.ExecutionDirectiveTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    [GraphType("recordPart")]
    public class DirectiveDocumentPartRecorderDirective : GraphDirective
    {
        public IDocumentPart RecordedDocumentPart { get; set; }

        [DirectiveLocations(DirectiveLocation.AllExecutionLocations)]
        public IGraphActionResult Execute()
        {
            this.RecordedDocumentPart = this.DirectiveTarget as IDocumentPart;
            return this.Ok();
        }
    }
}