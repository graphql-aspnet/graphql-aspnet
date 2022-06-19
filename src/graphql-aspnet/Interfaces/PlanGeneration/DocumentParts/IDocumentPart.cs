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
    using System.Collections.Generic;
    using GraphQL.AspNet.PlanGeneration.Document;

#pragma warning disable SA1623 // Property summary documentation should match accessors
    /// <summary>
    /// A general interface describing part of a query document and the document parts it may contain.
    /// </summary>
    public interface IDocumentPart
    {
        /// <summary>
        /// Assigns the provided part as being the parent of this part. Value can be null, not
        /// all implementations may allow a change of parent assignment.
        /// </summary>
        /// <param name="parent">The parent to assign.</param>
        internal void AssignParent(IDocumentPart parent);

        /// <summary>
        /// Gets the child parts declared on this instance, if any. Child parts may include
        /// child fields, input arguments, variable collections, assigned directives etc.
        /// </summary>
        /// <value>The children.</value>
        IEnumerable<IDocumentPart> Children { get; }

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
    }
}