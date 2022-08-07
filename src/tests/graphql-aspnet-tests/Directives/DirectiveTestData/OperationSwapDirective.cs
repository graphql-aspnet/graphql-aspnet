// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Directives.DirectiveTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Schemas.TypeSystem;

    public class OperationSwapDirective : GraphDirective
    {
        [DirectiveLocations(DirectiveLocation.MUTATION)]
        public IGraphActionResult Execute()
        {
            // swap out the assigned query
            if (this.DirectiveTarget is IOperationDocumentPart op)
            {
                var query = this.Schema.Operations[GraphOperationType.Query];
                op.AssignGraphType(query);
            }

            return this.Ok();
        }
    }
}