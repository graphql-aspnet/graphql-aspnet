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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;

    /// <summary>
    /// A fragment that was parsed out of a submitted query document.
    /// </summary>
    [DebuggerDisplay("Named Fragment: {Name}")]
    internal class QueryFragment : IDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryFragment" /> class.
        /// </summary>
        /// <param name="fragmentNode">The fragment node.</param>
        public QueryFragment(NamedFragmentNode fragmentNode)
        {
            this.Node = Validation.ThrowIfNullOrReturn(fragmentNode, nameof(fragmentNode));
            this.Name = fragmentNode.FragmentName.ToString();
            this.ReferencedNamedFragments = new CharMemoryHashSet();
            this.TargetGraphTypeName = fragmentNode.TargetType.ToString();

            foreach (var node in this.Node.Children)
                this.BuildReferencedFragmentList(node);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryFragment" /> class.
        /// </summary>
        /// <param name="inlineFragmentNode">The inline fragment node.</param>
        public QueryFragment(FragmentNode inlineFragmentNode)
        {
            this.Node = Validation.ThrowIfNullOrReturn(inlineFragmentNode, nameof(inlineFragmentNode));
            this.Name = string.Empty;
            this.ReferencedNamedFragments = new CharMemoryHashSet();
            this.IsReferenced = true;
            this.TargetGraphTypeName = inlineFragmentNode.TargetType.ToString();

            foreach (var node in this.Node.Children)
                this.BuildReferencedFragmentList(node);
        }

        /// <summary>
        /// Walk the node tree of this instance looking for any pointers to named fragments and store a list
        /// of all those instances that were/are found.
        /// </summary>
        /// <param name="inspectedNode">A node in the node tree to inspect.</param>
        private void BuildReferencedFragmentList(SyntaxNode inspectedNode)
        {
            if (inspectedNode is FragmentSpreadNode fsn)
                this.ReferencedNamedFragments.Add(fsn.PointsToFragmentName);

            if (inspectedNode.Children != null)
            {
                foreach (var node in inspectedNode.Children)
                    this.BuildReferencedFragmentList(node);
            }
        }

        /// <summary>
        /// Marks this fragment as being referenced and used in at least one operation in the document.
        /// </summary>
        public void MarkAsReferenced()
        {
            this.IsReferenced = true;
        }

        /// <summary>
        /// Gets the unique name of this reference in the collection.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the node this reference points to.
        /// </summary>
        /// <value>The node.</value>
        public FragmentNode Node { get; }

        /// <summary>
        /// Gets or sets the graphtype this named fragment is restricted to when set. If null, there are no restrictions to
        /// where this named fragment can be used.
        /// </summary>
        /// <value>The type of the graph.</value>
        public IGraphType GraphType { get; set; }

        /// <summary>
        /// Gets a collection of named fragments to be spread within this fragment.
        /// </summary>
        /// <value>The set of named fragments referenced by this instance.</value>
        public CharMemoryHashSet ReferencedNamedFragments { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is referenced by an operation in the query document.
        /// </summary>
        /// <value><c>true</c> if this instance is referenced; otherwise, <c>false</c>.</value>
        public bool IsReferenced { get; private set; }

        /// <summary>
        /// Gets the child parts declared in this instance.
        /// </summary>
        /// <value>The children.</value>
        public IEnumerable<IDocumentPart> Children => Enumerable.Empty<IDocumentPart>();

        /// <summary>
        /// Gets the name of the target graph type if any.
        /// </summary>
        /// <value>The name of the target graph type.</value>
        public string TargetGraphTypeName { get; }
    }
}