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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// A collection of variable references for a single operation that are used to validate any variable
    /// usage within an operation.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("Count = {Count}")]
    internal class DocumentVariableCollection : IVariableCollectionDocumentPart
    {
        private readonly Dictionary<string, IVariableDocumentPart> _variables;
        private readonly HashSet<string> _referencedVariables;
        private HashSet<string> _duplicates;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentVariableCollection" /> class.
        /// </summary>
        /// <param name="ownerOperation">The operation that owns this variable collection.
        /// this instance will be automatically populated by the variables it contains.</param>
        public DocumentVariableCollection(IOperationDocumentPart ownerOperation)
        {
            _variables = new Dictionary<string, IVariableDocumentPart>();
            _referencedVariables = new HashSet<string>();
            this.Operation = Validation.ThrowIfNullOrReturn(ownerOperation, nameof(ownerOperation));
        }

        /// <summary>
        /// Adds the specified variable to the collection.
        /// </summary>
        /// <param name="variable">The variable.</param>
        public void Add(IVariableDocumentPart variable)
        {
            Validation.ThrowIfNull(variable, nameof(variable));
            if (!_variables.ContainsKey(variable.Name))
            {
                _variables.Add(variable.Name, variable);
            }
            else
            {
                _duplicates = _duplicates ?? new HashSet<string>();
                _duplicates.Add(variable.Name);
            }
        }

        /// <inheritdoc />
        public bool Contains(string key)
        {
            return _variables.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out IVariableDocumentPart value)
        {
            return _variables.TryGetValue(key, out value);
        }

        /// <summary>
        /// Determines whether the specified variable key represents a variable that was duplicated
        /// in the operation.
        /// </summary>
        /// <param name="variableName">The key.</param>
        /// <returns><c>true</c> if the specified key is duplicate; otherwise, <c>false</c>.</returns>
        public bool IsDuplicated(string variableName)
        {
            variableName = Validation.ThrowIfNullWhiteSpaceOrReturn(variableName, nameof(variableName));
            return _duplicates != null && _duplicates.Contains(variableName);
        }

        public void MarkAsReferenced(string variableName)
        {
            variableName = Validation.ThrowIfNullWhiteSpaceOrReturn(variableName, nameof(variableName));
            if (_variables.ContainsKey(variableName))
                _referencedVariables.Add(variableName);
        }

        public bool IsReferenced(string variableName)
        {
            variableName = Validation.ThrowIfNullWhiteSpaceOrReturn(variableName, nameof(variableName));
            return _referencedVariables.Contains(variableName);
        }

        public IEnumerator<IVariableDocumentPart> GetEnumerator()
        {
            return _variables.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void ClearReferences()
        {
            _referencedVariables.Clear();
        }

        /// <inheritdoc />
        public IOperationDocumentPart Operation { get; }

        /// <inheritdoc />
        public int Count => _variables.Count;

        public IEnumerable<string> Duplicates => _duplicates ?? Enumerable.Empty<string>();

        public IEnumerable<IVariableDocumentPart> UnreferencedVariables
        {
            get
            {
                foreach (var variable in _variables.Values)
                {
                    if (!_referencedVariables.Contains(variable.Name))
                        yield return variable;
                }
            }
        }

        /// <inheritdoc />
        public IVariableDocumentPart this[string key] => _variables[key];
    }
}