// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts.SuppliedValues
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A base class providing common functionality for any "supplied value" to an argument
    /// or variable in a query document.
    /// </summary>
    [Serializable]
    internal abstract class DocumentSuppliedValue : ISuppliedValueDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentSuppliedValue" /> class.
        /// </summary>
        /// <param name="node">The node that represents this value in the user's query document.</param>
        protected DocumentSuppliedValue(SyntaxNode node)
        {
            this.ValueNode = Validation.ThrowIfNullOrReturn(node, nameof(node));
        }

        /// <inheritdoc />
        public virtual void AddChild(IDocumentPart child)
        {
            throw new InvalidOperationException($"{this.GetType().FriendlyName()} cannot contain children of type '{child?.GetType()}'");
        }

        /// <inheritdoc />
        public IAssignableValueDocumentPart Owner { get;  set; }

        /// <inheritdoc />
        public ISuppliedValueDocumentPart ParentValue { get;  set; }

        /// <inheritdoc />
        public SyntaxNode ValueNode { get; }

        /// <inheritdoc />
        public virtual IEnumerable<IDocumentPart> Children => Enumerable.Empty<IDocumentPart>();

        /// <inheritdoc />
        public virtual DocumentPartType PartType => DocumentPartType.SuppliedValue;
    }
}