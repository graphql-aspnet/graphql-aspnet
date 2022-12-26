// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.DocumentParts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts.Common;

    /// <summary>
    /// A collection of directives parsed and assigned to a given document part.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    internal class DocumentDirectiveCollection : IDirectiveCollectionDocumentPart
    {
        private List<IDirectiveDocumentPart> _directives;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDirectiveCollection"/> class.
        /// </summary>
        /// <param name="owner">The owner document part to which the directives are assigned.</param>
        public DocumentDirectiveCollection(IDocumentPart owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _directives = new List<IDirectiveDocumentPart>();
        }

        /// <summary>
        /// Adds a new directive to the collection.
        /// </summary>
        /// <param name="directive">The directive.</param>
        public void AddDirective(IDirectiveDocumentPart directive)
        {
            Validation.ThrowIfNull(directive, nameof(directive));
            _directives.Add(directive);
        }

        /// <inheritdoc />
        public IDirectiveDocumentPart this[int index] => _directives[index];

        /// <inheritdoc />
        public int Count => _directives.Count;

        /// <inheritdoc />
        public IDocumentPart Owner { get; }

        /// <inheritdoc />
        public IEnumerator<IDirectiveDocumentPart> GetEnumerator()
        {
            return _directives.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}