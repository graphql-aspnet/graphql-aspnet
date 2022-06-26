// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An instance of a referenced directive in a query document.
    /// </summary>
    [DebuggerDisplay("Directive {DirectiveName}")]
    internal class DocumentDirective : DocumentPartBase, IDirectiveDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDirective" /> class.
        /// </summary>
        /// <param name="parentPart">The parent part that owns this directive.</param>
        /// <param name="node">The node denoting the directive.</param>
        public DocumentDirective(IDocumentPart parentPart, DirectiveNode node)
            : base(parentPart, node)
        {
            this.Location = parentPart.Node.AsDirectiveLocation();
            this.DirectiveName = node.DirectiveName.ToString();
        }

        /// <inheritdoc />
        public DirectiveLocation Location { get; }

        /// <inheritdoc />
        public string DirectiveName { get; }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.Directive;
    }
}