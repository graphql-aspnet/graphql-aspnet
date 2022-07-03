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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    internal class DocumentDirectiveCollection : IDirectiveCollectionDocumentPart
    {
        private List<IDirectiveDocumentPart> _directives;

        public DocumentDirectiveCollection(IDocumentPart owner)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            _directives = new List<IDirectiveDocumentPart>();
        }
        public void AddDirective(IDirectiveDocumentPart directive)
        {
            Validation.ThrowIfNull(directive, nameof(directive));
            _directives.Add(directive);
        }

        public IDirectiveDocumentPart this[int index] => _directives[index];

        public int Count => _directives.Count;

        public IDocumentPart Owner { get; }

        public IEnumerator<IDirectiveDocumentPart> GetEnumerator()
        {
            return _directives.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }
}