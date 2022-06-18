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
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An instance of a referenced directive in a query document.
    /// </summary>
    [DebuggerDisplay("Directive {Directive.Name}")]
    public class DocumentDirective : IDirectiveDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDirective" /> class.
        /// </summary>
        /// <param name="node">The node denoting the directive.</param>
        /// <param name="directive">The directive pulled from the target schema.</param>
        /// <param name="location">The location in the source document where this directive instance was seen.</param>
        public DocumentDirective(DirectiveNode node, IDirective directive, DirectiveLocation location)
        {
            this.Node = Validation.ThrowIfNullOrReturn(node, nameof(node));
            this.Directive = Validation.ThrowIfNullOrReturn(directive, nameof(directive));
            this.Location = location;
            this.Arguments = new DocumentInputArgumentCollection();
        }

        /// <inheritdoc />
        public void AddArgument(IQueryArgumentDocumentPart argument)
        {
            this.Arguments.AddArgument(argument);
        }

        /// <inheritdoc />
        public DirectiveNode Node { get; }

        /// <inheritdoc />
        public IDirective Directive { get; }

        /// <inheritdoc />
        public DirectiveLocation Location { get; }

        /// <inheritdoc />
        public string Name => this.Directive.Name;

        /// <inheritdoc />
        public IQueryInputArgumentCollectionDocumentPart Arguments { get; }

        /// <inheritdoc />
        public IEnumerable<IDocumentPart> Children
        {
            get
            {
                foreach (var argument in this.Arguments.Values)
                    yield return argument;
            }
        }
    }
}