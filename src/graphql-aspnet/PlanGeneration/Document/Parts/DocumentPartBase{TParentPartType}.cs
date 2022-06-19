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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// A base class with common functionality of various document part implementations.
    /// </summary>
    /// <typeparam name="TParentPartType">The required part type of the parent of this item.</typeparam>
    internal abstract class DocumentPartBase<TParentPartType> : DocumentPartBase
        where TParentPartType : IDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPartBase{TParentPartType}" /> class.
        /// </summary>
        /// <param name="parent">The parent document part, if any, that owns this instance.</param>
        protected DocumentPartBase(IDocumentPart parent = null)
            : base(parent)
        {
        }

        /// <inheritdoc />
        public override void AssignParent(IDocumentPart parent)
        {
            if (!(parent is TParentPartType))
            {
                throw new InvalidOperationException(
                    $"The parent of a {nameof(DocumentFragment)} must implement " +
                    $"{typeof(TParentPartType).FriendlyName()}.");
            }

            this.Parent = parent;
        }
    }
}