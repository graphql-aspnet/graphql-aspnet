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
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;

    /// <summary>
    /// A collection of ordered <see cref="IDirectiveDocumentPart" /> items.
    /// </summary>
    [Serializable]
    internal class DocumentDirectiveCollection : IEnumerable<IDirectiveDocumentPart>
    {
        private readonly List<(int Rank, IDirectiveDocumentPart Directive)> _rankedDirectives;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDirectiveCollection"/> class.
        /// </summary>
        public DocumentDirectiveCollection()
        {
            _rankedDirectives = new List<(int Rank, IDirectiveDocumentPart Directive)>();
        }

        /// <summary>
        /// Adds the specified directive at the specificed relative rank. Ranks can be duplicated.
        /// </summary>
        /// <param name="rank">The rank to insert the directive at.</param>
        /// <param name="directive">The directive to insert.</param>
        public void Add(int rank, IDirectiveDocumentPart directive)
        {
            _rankedDirectives.Add((rank, directive));
        }

        /// <inheritdoc />
        public IEnumerator<IDirectiveDocumentPart> GetEnumerator()
        {
            return _rankedDirectives
            .OrderBy(x => x.Rank)
            .Select(x => x.Directive)
            .ToList()
            .GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}