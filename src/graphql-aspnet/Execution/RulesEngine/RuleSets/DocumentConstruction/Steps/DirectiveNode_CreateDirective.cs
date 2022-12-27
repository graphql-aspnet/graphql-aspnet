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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    /// <summary>
    /// Creates and assigns the active directive node to be the active <see cref="IDirectiveDocumentPart"/> on the context.
    /// </summary>
    internal class DirectiveNode_CreateDirective : DocumentConstructionStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectiveNode_CreateDirective"/> class.
        /// </summary>
        public DirectiveNode_CreateDirective()
            : base(SyntaxNodeType.Directive)
        {
        }

        /// <inheritdoc />
        public override bool Execute(ref DocumentConstructionContext context)
        {
            var node = context.ActiveNode;

            var docPart = new DocumentDirective(
                context.ParentPart,
                context.SourceText.Slice(node.PrimaryValue.TextBlock),
                node.Location);

            var directive = context.Schema.KnownTypes.FindDirective(docPart.DirectiveName);
            docPart.AssignGraphType(directive);

            context = context.AssignPart(docPart);
            return true;
        }
    }
}