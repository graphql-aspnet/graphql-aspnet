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
    using System;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    [GraphType("argInjector")]
    public class ArgumentIntjectorDirective : GraphDirective
    {
        [DirectiveLocations(AspNet.Schemas.TypeSystem.DirectiveLocation.AllExecutionLocations)]
        public IGraphActionResult Execute()
        {
            // randomly ineject an argument into the active part
            if (this.DirectiveTarget is IDocumentPart dp)
            {
                dp.Children.Add(new DocumentInputArgument(
                    dp,
                    new InputItemNode(SourceLocation.None, string.Empty.AsMemory()),
                    new AspNet.Schemas.GraphTypeExpression("String")));
            }

            return this.Ok();
        }
    }
}