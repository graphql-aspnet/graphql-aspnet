﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts
{
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Schema;

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
        /// Forces this document part to do an internal refresh in a manner suitable to its function.
        /// </summary>
        void Refresh();

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
        /// Gets a common description that accurately identifies this document part. This value is
        /// controlled by the internal document system and largely used for debugging.
        /// It is not parsable or settable by the user.
        /// </summary>
        /// <value>The description of this document part.</value>
        string Description { get; }

        /// <summary>
        /// Gets the object pointing to where this
        /// document part originated in the source text.
        /// </summary>
        /// <value>The location in the source text of this entity.</value>
        SourceOrigin Origin { get; }
    }
}