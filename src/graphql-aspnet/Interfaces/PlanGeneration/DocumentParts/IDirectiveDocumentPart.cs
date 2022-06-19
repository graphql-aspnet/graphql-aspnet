// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts
{
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A directive, indicated in the supplied query document, to be executed by the runtime
    /// against the <see cref="IDocumentPart"/> to which its assigned.
    /// </summary>
    public interface IDirectiveDocumentPart : IInputArgumentContainerDocumentPart, IDocumentPart
    {
        /// <summary>
        /// Gets the directive from the graph schema this instance is referencing.
        /// </summary>
        /// <value>The directive, from the schema, referenced in the document.</value>
        IDirective Directive { get; }

        /// <summary>
        /// Gets the location in the source document where this directive instance was seen.
        /// </summary>
        /// <value>The location.</value>
        DirectiveLocation Location { get; }

        /// <summary>
        /// Gets the name of the directive as it exists on the schema.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the node that generated this query directive instance.
        /// </summary>
        /// <value>The node.</value>
        DirectiveNode Node { get; }
    }
}