// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Configuration.SchemaOptionsTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    internal class SimpleLateBoundObjectDirective : GraphDirective
    {
        public static int TOTAL_INVOCATIONS = 0;

        [DirectiveLocations(DirectiveLocation.OBJECT)]
        public IGraphActionResult PerformOperationon()
        {
            if (this.DirectiveTarget is IObjectGraphType graphType && graphType.ObjectType == typeof(ObjectForLateBoundDirective))
            {
                TOTAL_INVOCATIONS += 1;
            }

            // dont actually do anything, this is just a test for inclusion
            return this.Ok();
        }
    }
}