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
    /// on their query document. Keyed by the return value (a.k.a. the field alias) requested by the user.
    /// </summary>
    [DebuggerDisplay("Graph Type: {GraphType.Name}, Fields = {Count}")]
    public class FieldSelectionSet : IReadOnlyList<FieldSelection>, IDocumentPart
    {
        private readonly CharMemoryHashSet _knownFieldAliases;
        private readonly List<FieldSelection> _fields;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldSelectionSet" /> class.
        /// </summary>
        /// <param name="graphType">The graph type this selection set is acting on.</param>
        /// <param name="rootPath">The document specific root path under which all fields should be nested in this selection set.</param>
        public FieldSelectionSet(IGraphType graphType, SourcePath rootPath)
        {
            this.GraphType = Validation.ThrowIfNullOrReturn(graphType, nameof(graphType));
            _knownFieldAliases = new CharMemoryHashSet();
            _fields = new List<FieldSelection>();

            Validation.ThrowIfNull(rootPath, nameof(rootPath));
            this.RootPath = rootPath.Clone();
        }

        /// <summary>
        /// Adds the field selection to this instance.
        /// </summary>
        /// <param name="newField">The new field.</param>
        public virtual void AddFieldSelection(FieldSelection newField)
        {
            _knownFieldAliases.Add(newField.Alias);
            _fields.Add(newField);
            newField.UpdatePath(this.RootPath);
        }

        /// <summary>
        /// Returns a value indicating if any field in this instance matches the given alias. This method is a fast lookup
        /// and does not traverse the list of nodes.
        /// </summary>
        /// <param name="alias">The alias to search for.</param>
        /// <returns><c>true</c> if the alias was found,; otherwise, <c>false</c>.</returns>
        public virtual bool ContainsAlias(in ReadOnlyMemory<char> alias)
        {
            return _knownFieldAliases.Contains(alias);
        }

        /// <summary>
        /// Searches the given selection set for any fields with the given output alias/name.
        /// </summary>
        /// <param name="alias">The alias to search for.</param>
        /// <returns>A list of fields with the given name or an empty list.</returns>
        public IEnumerable<FieldSelection> FindFieldsOfAlias(ReadOnlyMemory<char> alias)
        {
            return _fields.Where(x => x.Alias.Span.SequenceEqual(alias.Span));
        }

        /// <summary>
        /// Gets the graph type from which the requested fields will be extracted.
        /// </summary>
        /// <value>The type of the graph.</value>
        public IGraphType GraphType { get; }

        /// <summary>
        /// Gets the child parts declared in this instance.
        /// </summary>
        /// <value>The children.</value>
        public IEnumerable<IDocumentPart> Children
        {
            get
            {
                foreach (var field in _fields)
                    yield return field;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<FieldSelection> GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <value>The count.</value>
        public virtual int Count => _fields.Count;

        /// <summary>
        /// Gets the root path of all fields added to this selection set.
        /// </summary>
        /// <value>The root path.</value>
        public SourcePath RootPath { get; }

        /// <summary>
        /// Gets the <see cref="FieldSelection"/> at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>FieldSelection.</returns>
        public virtual FieldSelection this[int index] => _fields[index];
    }
}