namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    internal class DocumentVariableUsageCollection : IVariableUsageCollectionDocumentPart
    {
        private Dictionary<string, List<IVariableUsageDocumentPart>> _references;

        public DocumentVariableUsageCollection(IReferenceDocumentPart owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _references = new Dictionary<string, List<IVariableUsageDocumentPart>>();
        }

        public void Add(IVariableUsageDocumentPart variableUsage)
        {
            Validation.ThrowIfNull(variableUsage, nameof(variableUsage));

            var name = variableUsage.VariableName.ToString();
            if (!_references.ContainsKey(name))
                _references.Add(name, new List<IVariableUsageDocumentPart>());

            _references[name].Add(variableUsage);
            this.Count += 1;
        }

        public IEnumerable<IVariableUsageDocumentPart> FindReferences(string variableName)
        {
            variableName = Validation.ThrowIfNullOrReturn(variableName, nameof(variableName));
            if (_references.ContainsKey(variableName))
                return _references[variableName];

            return Enumerable.Empty<IVariableUsageDocumentPart>();
        }

        public bool HasUsages(string variableName)
        {
            variableName = Validation.ThrowIfNullOrReturn(variableName, nameof(variableName));
            return _references.ContainsKey(variableName);
        }

        public IEnumerator<IVariableUsageDocumentPart> GetEnumerator()
        {
            return _references.Values.SelectMany(x => x).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count { get; private set; }

        public IDocumentPart Owner { get; }
    }
}
