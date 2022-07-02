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

    internal class DocumentFragmentSpreadCollection : IFragmentSpreadCollectionDocumentPart
    {
        private Dictionary<string, List<IFragmentSpreadDocumentPart>> _spreads;

        public DocumentFragmentSpreadCollection(IDocumentPart owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _spreads = new Dictionary<string, List<IFragmentSpreadDocumentPart>>();
        }

        public void Add(IFragmentSpreadDocumentPart fragmentSpread)
        {
            Validation.ThrowIfNull(fragmentSpread, nameof(fragmentSpread));
            if (fragmentSpread.FragmentName.Length > 0)
            {
                var fragName = fragmentSpread.FragmentName.ToString();
                if (!_spreads.ContainsKey(fragName))
                    _spreads.Add(fragName, new List<IFragmentSpreadDocumentPart>());

                _spreads[fragName].Add(fragmentSpread);
                this.Count += 1;
            }
        }

        public IEnumerable<IFragmentSpreadDocumentPart> FindReferences(string fragmentName)
        {
            fragmentName = Validation.ThrowIfNullWhiteSpaceOrReturn(fragmentName, nameof(fragmentName));
            if (_spreads.ContainsKey(fragmentName))
                return _spreads[fragmentName];

            return Enumerable.Empty<IFragmentSpreadDocumentPart>();
        }

        public bool IsSpread(string fragmentName)
        {
            fragmentName = Validation.ThrowIfNullWhiteSpaceOrReturn(fragmentName, nameof(fragmentName));
            return _spreads.ContainsKey(fragmentName);
        }

        public IEnumerator<IFragmentSpreadDocumentPart> GetEnumerator()
        {
            return _spreads.Values.SelectMany(x => x).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count { get; private set; }

        public IDocumentPart Owner { get; }

    }
}
