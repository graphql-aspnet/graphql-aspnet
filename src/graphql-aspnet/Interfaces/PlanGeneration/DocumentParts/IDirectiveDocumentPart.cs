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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A directive, indicated in the supplied query document, to be executed by the runtime
    /// against the <see cref="IDocumentPart"/> to which its assigned.
    /// </summary>
    public interface IDirectiveDocumentPart : ISecureDocumentPart, IDocumentPart
    {
        /// <summary>
        /// Gets the location in the source document where this directive instance was declared.
        /// </summary>
        /// <value>The source document location.</value>
        DirectiveLocation Location { get; }

        /// <summary>
        /// Gets the name of the directive pointed to by this document part. This may or
        /// may not be a valid directive on the target schema.
        /// </summary>
        /// <value>The name of the directive indicated by this instance.</value>
        string DirectiveName { get; }

        /// <summary>
        /// Gets the collection of arguments defined on this directive instance.
        /// </summary>
        /// <value>The arguments.</value>
        IInputArgumentCollectionDocumentPart Arguments { get; }
    }
}