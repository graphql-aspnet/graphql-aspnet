// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentConstruction.Steps
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentConstruction.Common;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// Adds a new variable to the current context and active variable collection.
    /// </summary>
    internal class QueryVariable_CreateNewVariable : DocumentConstructionStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryVariable_CreateNewVariable"/> class.
        /// </summary>
        public QueryVariable_CreateNewVariable()
            : base(SyntaxNodeType.Variable)
        {
        }

        /// <inheritdoc />
        public override bool Execute(ref DocumentConstructionContext context)
        {
            var node = context.ActiveNode;

            var variableName = context.SourceText.Slice(node.PrimaryValue.TextBlock).ToString();
            var typeExpressionText = context.SourceText.Slice(node.SecondaryValue.TextBlock).ToString();
            var typeExpression = GraphTypeExpression.FromDeclaration(typeExpressionText);

            var docPart = new DocumentVariable(
                context.ParentPart,
                variableName,
                typeExpression,
                node.Location);

            var graphType = context.Schema.KnownTypes.FindGraphType(docPart.TypeExpression.TypeName);
            docPart.AssignGraphType(graphType);

            context = context.AssignPart(docPart);
            return true;
        }
    }
}