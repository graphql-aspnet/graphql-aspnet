// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Contexts
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// A set of fields currently "in scope" that can be acted on as a group.
    /// </summary>
    internal class DocumentScope : IEnumerable<IDocumentPart>
    {
        private readonly DocumentScope _parentScope;
        private readonly List<IDocumentPart> _parts;
        private readonly List<QueryDirective> _directives;
        private readonly int _scopeRank;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentScope" /> class.
        /// </summary>
        /// <param name="parentScope">The parent scope for which this scope will be a child. Some changes made to this
        /// instance will effect all members of the parent, but not vice versa.</param>
        /// <param name="part">An optional document to add automatically to this scope.</param>
        public DocumentScope(DocumentScope parentScope = null, IDocumentPart part = null)
        {
            _parentScope = parentScope;
            _scopeRank = (_parentScope?._scopeRank ?? 0) + 5;
            _parts = new List<IDocumentPart>();
            _directives = new List<QueryDirective>();
            if (part != null)
                this.AddNewPart(part);
        }

        /// <summary>
        /// Adds the new part to this scope (from a child scope) and
        /// adds any pending pieces to it per the requirements of the specific part being added.
        /// </summary>
        /// <param name="part">The part to add to this scope.</param>
        private void AddNewPart(IDocumentPart part)
        {
            _parts.Add(part);
            _parentScope?.AddNewPart(part);

            if (part is IDirectiveContainerDocumentPart dcp)
            {
                foreach (var directive in this.Directives)
                {
                    dcp.InsertDirective(directive, _scopeRank);
                }
            }

            if (part is ITargetedDocumentPart tdp)
                tdp.TargetGraphType = tdp.TargetGraphType ?? this.TargetGraphType;
        }

        /// <summary>
        /// Adds the new directive to every field in this scope and will automatically preappend it to any new fields that are added later.
        /// Directives are propagated to parent scopres.
        /// </summary>
        /// <param name="queryDirective">The query directive.</param>
        public void InsertDirective(QueryDirective queryDirective)
        {
            _directives.Add(queryDirective);
            foreach (var part in _parts.OfType<IDirectiveContainerDocumentPart>())
                part.InsertDirective(queryDirective, _scopeRank);
        }

        /// <summary>
        /// Restricts any new fields added to the of the current field scope to the provided graph type. Graph Field restrictions are
        /// not propagated to parent scopes.
        /// </summary>
        /// <param name="graphType">Type of the graph.</param>
        public void RestrictFieldsToGraphType(IGraphType graphType)
        {
            this.TargetGraphType = graphType;
            foreach (var part in _parts.OfType<ITargetedDocumentPart>())
            {
                part.TargetGraphType = part.TargetGraphType ?? this.TargetGraphType;
            }
        }

        /// <summary>
        /// Gets the graph type every item in scope should target, if any.
        /// </summary>
        /// <value>The type of the target graph.</value>
        public IGraphType TargetGraphType { get; private set; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IDocumentPart> GetEnumerator()
        {
            return _parts.GetEnumerator();
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
        /// Gets the directives currently attached to this field scope.
        /// </summary>
        /// <value>The directives.</value>
        public IReadOnlyList<QueryDirective> Directives => _directives;
    }
}