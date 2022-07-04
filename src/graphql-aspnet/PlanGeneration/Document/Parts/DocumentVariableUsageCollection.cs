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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    /// <summary>
    /// Class DocumentVariableUsageCollection.
    /// </summary>
    internal class DocumentVariableUsageCollection : IVariableUsageCollectionDocumentPart
    {
        private Dictionary<string, List<IVariableUsageDocumentPart>> _references;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentVariableUsageCollection"/> class.
        /// </summary>
        /// <param name="owner">The owner of this usage collection.</param>
        public DocumentVariableUsageCollection(IReferenceDocumentPart owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _references = new Dictionary<string, List<IVariableUsageDocumentPart>>();
        }

        /// <summary>
        /// Adds the specified variable usage to this collection.
        /// </summary>
        /// <param name="variableUsage">The variable usage.</param>
        public void Add(IVariableUsageDocumentPart variableUsage)
        {
            Validation.ThrowIfNull(variableUsage, nameof(variableUsage));

            var name = variableUsage.VariableName.ToString();
            if (!_references.ContainsKey(name))
                _references.Add(name, new List<IVariableUsageDocumentPart>());

            _references[name].Add(variableUsage);
            this.Count += 1;
        }

        /// <inheritdoc />
        public IEnumerable<IVariableUsageDocumentPart> FindReferences(string variableName)
        {
            variableName = Validation.ThrowIfNullOrReturn(variableName, nameof(variableName));
            if (_references.ContainsKey(variableName))
                return _references[variableName];

            return Enumerable.Empty<IVariableUsageDocumentPart>();
        }

        /// <inheritdoc />
        public bool HasUsages(string variableName)
        {
            variableName = Validation.ThrowIfNullOrReturn(variableName, nameof(variableName));
            return _references.ContainsKey(variableName);
        }

        /// <inheritdoc />
        public IEnumerator<IVariableUsageDocumentPart> GetEnumerator()
        {
            return _references.Values.SelectMany(x => x).GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public int Count { get; private set; }

        /// <inheritdoc />
        public IDocumentPart Owner { get; }
    }
}