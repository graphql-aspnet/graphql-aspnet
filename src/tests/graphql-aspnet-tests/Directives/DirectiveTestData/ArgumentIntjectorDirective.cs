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
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using Moq;

    [GraphType("argInjector")]
    public class ArgumentIntjectorDirective : GraphDirective
    {
        [DirectiveLocations(AspNet.Schemas.TypeSystem.DirectiveLocation.AllExecutionLocations)]
        public IGraphActionResult Execute()
        {
            var arg = new Mock<IGraphArgument>();
            arg.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            arg.Setup(x => x.Name).Returns("bobArg");

            // randomly ineject an argument into the active part
            if (this.DirectiveTarget is IDocumentPart dp)
            {
                dp.Children.Add(new DocumentInputArgument(
                    dp,
                    arg.Object,
                    "bobArg",
                    SourceLocation.None));
            }

            return this.Ok();
        }
    }
}