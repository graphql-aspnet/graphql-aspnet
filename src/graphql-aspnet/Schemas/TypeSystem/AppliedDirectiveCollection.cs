// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A collection of directives applied to some <see cref="ISchemaItem"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class AppliedDirectiveCollection : IAppliedDirectiveCollection
    {
        private HashSet<IAppliedDirective> _appliedDirectives;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppliedDirectiveCollection"/> class.
        /// </summary>
        public AppliedDirectiveCollection()
        {
            _appliedDirectives = new HashSet<IAppliedDirective>(AppliedDirectiveEqualityComparer.Instance);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppliedDirectiveCollection"/> class.
        /// </summary>
        /// <param name="parent">The parent schema item that owns this collection.</param>
        public AppliedDirectiveCollection(ISchemaItem parent)
            : this()
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
        }

        /// <inheritdoc />
        public IAppliedDirectiveCollection Clone(ISchemaItem newParent)
        {
            var clone = new AppliedDirectiveCollection(newParent);
            clone._appliedDirectives = new HashSet<IAppliedDirective>(_appliedDirectives, AppliedDirectiveEqualityComparer.Instance);
            return clone;
        }

        /// <inheritdoc />
        public void Add(IAppliedDirective directive)
        {
            Validation.ThrowIfNull(directive, nameof(directive));
            _appliedDirectives.Add(directive);
        }

        /// <inheritdoc />
        public ISchemaItem Parent { get; }

        /// <inheritdoc />
        public int Count => _appliedDirectives.Count;

        /// <inheritdoc />
        public IEnumerator<IAppliedDirective> GetEnumerator()
        {
            return _appliedDirectives.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}