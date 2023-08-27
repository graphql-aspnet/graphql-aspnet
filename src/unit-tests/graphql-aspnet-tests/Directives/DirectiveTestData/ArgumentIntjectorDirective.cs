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
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using NSubstitute;

    [GraphType("argInjector")]
    public class ArgumentIntjectorDirective : GraphDirective
    {
        [DirectiveLocations(AspNet.Schemas.TypeSystem.DirectiveLocation.AllExecutionLocations)]
        public IGraphActionResult Execute()
        {
            var arg = Substitute.For<IGraphArgument>();
            arg.TypeExpression.Returns(new GraphTypeExpression("String"));
            arg.Name.Returns("bobArg");

            // randomly ineject an argument into the active part
            if (this.DirectiveTarget is IDocumentPart dp)
            {
                dp.Children.Add(new DocumentInputArgument(
                    dp,
                    arg,
                    "bobArg",
                    SourceLocation.None));
            }

            return this.Ok();
        }
    }
}