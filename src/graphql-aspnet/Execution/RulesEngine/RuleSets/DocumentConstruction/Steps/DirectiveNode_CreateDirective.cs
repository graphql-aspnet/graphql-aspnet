// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstruction
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing2.DocumentGeneration.DocumentConstruction.Common;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

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