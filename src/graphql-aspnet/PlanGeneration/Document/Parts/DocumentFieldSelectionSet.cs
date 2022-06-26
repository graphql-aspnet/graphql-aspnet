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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A collection of fields (from a <see cref="IGraphType"/>) that are requested by a user and defined
    /// on their query document. Selected fields are keyed by the return value (a.k.a. the field alias) requested by the user.
    /// </summary>
    [DebuggerDisplay("FIELD SET: Graph Type: {GraphType.Name}, Fields = {Count}")]
    internal class DocumentFieldSelectionSet : DocumentPartBase, IFieldSelectionSetDocumentPart, IDocumentPart
    {
        private readonly CharMemoryHashSet _knownFieldAliases;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFieldSelectionSet" /> class.
        /// </summary>
        /// <param name="parent">The parent document part that owns this set.</param>
        public DocumentFieldSelectionSet(IDocumentPart parent)
            : base(parent, EmptyNode.Instance)
        {
            _knownFieldAliases = new CharMemoryHashSet();
            this.AssignGraphType(parent.GraphType);
        }

        /// <inheritdoc />
        protected override void OnChildPartAdded(IDocumentPart childPart)
        {
            if (childPart is IFieldDocumentPart fds)
                _knownFieldAliases.Add(fds.Alias);
        }

        /// <inheritdoc />
        protected override void OnChildPartRemoved(IDocumentPart childPart)
        {
            if (childPart is IFieldDocumentPart fds)
                _knownFieldAliases.Remove(fds.Alias);
        }

        /// <inheritdoc />
        public virtual bool ContainsAlias(in ReadOnlyMemory<char> alias)
        {
            return _knownFieldAliases.Contains(alias);
        }

        /// <inheritdoc />
        public IEnumerable<IFieldDocumentPart> FindFieldsOfAlias(ReadOnlyMemory<char> alias)
        {
            return this.Children[DocumentPartType.Field]
                .OfType<IFieldDocumentPart>()
                .Where(x => x.Alias.Span.SequenceEqual(alias.Span));
        }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.FieldSelectionSet;

        /// <inheritdoc />
        public int Count => this.Children[DocumentPartType.Field]
                .OfType<IFieldDocumentPart>()
                .Count();

        public IFieldDocumentPart this[int index]
            => this.Children[DocumentPartType.Field][index] as IFieldDocumentPart;

        /// <inheritdoc />
        public IEnumerator<IFieldDocumentPart> GetEnumerator()
        {
            return this.Children[DocumentPartType.Field]
                .OfType<IFieldDocumentPart>()
                .GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}