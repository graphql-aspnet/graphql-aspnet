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

    /// <summary>
    /// A collection of fields (from a <see cref="IGraphType"/>) that are requested by a user and defined
    /// on their query document. Selected fields are keyed by the return value (a.k.a. the field alias) requested by the user.
    /// </summary>
    [DebuggerDisplay("Graph Type: {GraphType.Name}, Fields = {Count}")]
    public class DocumentFieldSelectionSet : IFieldSelectionSetDocumentPart, IDocumentPart
    {
        private readonly CharMemoryHashSet _knownFieldAliases;
        private readonly List<IFieldSelectionDocumentPart> _fields;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFieldSelectionSet" /> class.
        /// </summary>
        /// <param name="graphType">The graph type this selection set is acting on.</param>
        /// <param name="path">The document specific root path under which all fields should be nested in this selection set.</param>
        public DocumentFieldSelectionSet(IGraphType graphType, SourcePath path)
        {
            this.GraphType = Validation.ThrowIfNullOrReturn(graphType, nameof(graphType));
            _knownFieldAliases = new CharMemoryHashSet();
            _fields = new List<IFieldSelectionDocumentPart>();

            Validation.ThrowIfNull(path, nameof(path));
            this.Path = path.Clone();
        }

        /// <inheritdoc />
        public virtual void AddFieldSelection(IFieldSelectionDocumentPart newField)
        {
            _knownFieldAliases.Add(newField.Alias);
            _fields.Add(newField);
            newField.UpdatePath(this.Path);
        }

        /// <inheritdoc />
        public virtual bool ContainsAlias(in ReadOnlyMemory<char> alias)
        {
            return _knownFieldAliases.Contains(alias);
        }

        /// <inheritdoc />
        public IEnumerable<IFieldSelectionDocumentPart> FindFieldsOfAlias(ReadOnlyMemory<char> alias)
        {
            return _fields.Where(x => x.Alias.Span.SequenceEqual(alias.Span));
        }

        /// <inheritdoc />
        public IGraphType GraphType { get; }

        /// <inheritdoc />
        public IEnumerable<IDocumentPart> Children
        {
            get
            {
                foreach (var field in _fields)
                    yield return field;
            }
        }

        /// <inheritdoc />
        public IEnumerator<IFieldSelectionDocumentPart> GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public virtual int Count => _fields.Count;

        /// <inheritdoc />
        public SourcePath Path { get; }

        /// <inheritdoc />
        public virtual IFieldSelectionDocumentPart this[int index] => _fields[index];

        /// <inheritdoc />
        public DocumentPartType PartType => DocumentPartType.FieldSelectionSet;
    }
}