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
    public class QueryDirective : IQueryArgumentContainerDocumentPart, IDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryDirective" /> class.
        /// </summary>
        /// <param name="node">The node denoting the directive.</param>
        /// <param name="directive">The directive pulled from the target schema.</param>
        /// <param name="location">The location in the source document where this directive instance was seen.</param>
        public QueryDirective(DirectiveNode node, IDirectiveGraphType directive, DirectiveLocation location)
        {
            this.Node = Validation.ThrowIfNullOrReturn(node, nameof(node));
            this.Directive = Validation.ThrowIfNullOrReturn(directive, nameof(directive));
            this.Location = location;
            this.Arguments = new QueryInputArgumentCollection();
        }

        /// <summary>
        /// Adds the argument to the collection of arguments on this instance.
        /// </summary>
        /// <param name="argument">The argument.</param>
        public void AddArgument(QueryInputArgument argument)
        {
            this.Arguments.AddArgument(argument);
        }

        /// <summary>
        /// Gets the node that generated this query directive instance.
        /// </summary>
        /// <value>The node.</value>
        public DirectiveNode Node { get; }

        /// <summary>
        /// Gets the actual directive from the graph schema this instance is referencing.
        /// </summary>
        /// <value>The directive.</value>
        public IDirectiveGraphType Directive { get; }

        /// <summary>
        /// Gets the location in the source document where this directive was seen.
        /// </summary>
        /// <value>The location.</value>
        public DirectiveLocation Location { get; }

        /// <summary>
        /// Gets the name of the directive as it exists on the schema.
        /// </summary>
        /// <value>The name.</value>
        public string Name => this.Directive.Name;

        /// <summary>
        /// Gets a collection of arguments that were defined on the node to be passed to the directive.
        /// </summary>
        /// <value>The argments.</value>
        public QueryInputArgumentCollection Arguments { get; }

        /// <summary>
        /// Gets the child parts declared in this instance.
        /// </summary>
        /// <value>The children.</value>
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