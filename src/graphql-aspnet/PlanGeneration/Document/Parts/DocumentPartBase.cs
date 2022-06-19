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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// A base class with common functionality of various document part implementations.
    /// </summary>
    internal abstract class DocumentPartBase : IDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPartBase" /> class.
        /// </summary>
        /// <param name="parent">The parent document part, if any, that owns this instance.</param>
        protected DocumentPartBase(IDocumentPart parent = null)
        {
            this.Parent = parent;
        }

        /// <inheritdoc />
        public virtual void AssignParent(IDocumentPart parent)
        {
            throw new NotSupportedException(
                $"The type {this.GetType().FriendlyName()} does not " +
                $"support changing its parent.");
        }

        /// <inheritdoc />
        public abstract DocumentPartType PartType { get; }

        /// <inheritdoc />
        public virtual IEnumerable<IDocumentPart> Children => Enumerable.Empty<IDocumentPart>();

        /// <inheritdoc />
        public IDocumentPart Parent { get; protected set; }
    }
}