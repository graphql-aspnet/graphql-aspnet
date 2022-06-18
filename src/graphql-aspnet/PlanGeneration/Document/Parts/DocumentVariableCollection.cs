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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// A collection of variable references for a single operation that are used to validate any variable
    /// usage within an operation.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    public class DocumentVariableCollection : IQueryVariableCollectionDocumentPart, IDocumentPart
    {
        private readonly Dictionary<string, IQueryVariableDocumentPart> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentVariableCollection"/> class.
        /// </summary>
        public DocumentVariableCollection()
        {
            _variables = new Dictionary<string, IQueryVariableDocumentPart>();
        }

        /// <inheritdoc />
        public void AddVariable(IQueryVariableDocumentPart variable)
        {
            Validation.ThrowIfNull(variable, nameof(variable));
            _variables.Add(variable.Name, variable);
        }

        /// <inheritdoc />
        public int Count => _variables.Count;

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return _variables.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out IQueryVariableDocumentPart value)
        {
            return _variables.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, IQueryVariableDocumentPart>> GetEnumerator()
        {
            return _variables.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public IQueryVariableDocumentPart this[string key] => _variables[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _variables.Keys;

        /// <inheritdoc />
        public IEnumerable<IQueryVariableDocumentPart> Values => _variables.Values;

        /// <inheritdoc />
        public IEnumerable<IDocumentPart> Children
        {
            get
            {
                return _variables.Values;
            }
        }

        /// <inheritdoc />
        public DocumentPartType PartType => DocumentPartType.VariableCollection;
    }
}