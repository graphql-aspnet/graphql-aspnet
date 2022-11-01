// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common
{
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Document;

    /// <summary>
    /// A general interface describing part of a query document and the document parts it may contain.
    /// </summary>
    public interface IDocumentPart
    {
        /// <summary>
        /// Assigns a graph type to be the target <see cref="GraphType"/> of this part.
        /// </summary>
        /// <param name="graphType">Type of the graph.</param>
        void AssignGraphType(IGraphType graphType);

        /// <summary>
        /// Gets the child parts declared on this instance, if any. Child parts may include
        /// child fields, input arguments, variable collections, assigned directives etc.
        /// </summary>
        /// <value>The children.</value>
        IDocumentPartsCollection Children { get; }

        /// <summary>
        /// Gets the specific graph type representing this instance. (i.e. the directive, the field return type,
        /// the operation etc.)
        /// </summary>
        /// <value>The graph type assigned to this document part.</value>
        IGraphType GraphType { get; }

        /// <summary>
        /// Gets the type of this document part.
        /// </summary>
        /// <value>The type of the part.</value>
        DocumentPartType PartType { get; }

        /// <summary>
        /// Gets the parent part that owns this document part. May be null
        /// if this is a root level part.
        /// </summary>
        /// <value>The parent.</value>
        IDocumentPart Parent { get; }

        /// <summary>
        /// Gets the location in the original query text where this document part originated.
        /// </summary>
        /// <value>The source location.</value>
        SourceLocation SourceLocation { get; }

        /// <summary>
        /// Gets human friendly path that notes where in the document this part exists.
        /// </summary>
        /// <value>The path.</value>
        SourcePath Path { get; }

        /// <summary>
        /// Gets a set of attributes applied to this document part.
        /// </summary>
        /// <remarks>
        /// This is an arbitrary collection of key/value that can indicate different things
        /// to different consumers. Care should be taken to ensure keys are globally unique for
        /// all potential key providers.
        /// </remarks>
        /// <value>Additional attributes applied to this document part.</value>
        MetaDataCollection Attributes { get; }

        /// <summary>
        /// Gets a common description that accurately identifies this document part. This value is
        /// controlled by the internal document system and largely used for debugging.
        /// It is not parsable or settable by the user.
        /// </summary>
        /// <value>The description of this document part.</value>
        string Description { get; }
    }
}