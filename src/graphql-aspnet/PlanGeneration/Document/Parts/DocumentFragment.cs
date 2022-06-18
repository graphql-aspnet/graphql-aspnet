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
    internal class DocumentFragment : IFragmentDocumentPart, IDocumentPart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFragment" /> class.
        /// </summary>
        /// <param name="fragmentNode">The fragment node.</param>
        public DocumentFragment(NamedFragmentNode fragmentNode)
        {
            this.Node = Validation.ThrowIfNullOrReturn(fragmentNode, nameof(fragmentNode));
            this.Name = fragmentNode.FragmentName.ToString();
            this.ReferencedNamedFragments = new CharMemoryHashSet();
            this.TargetGraphTypeName = fragmentNode.TargetType.ToString();

            foreach (var node in this.Node.Children)
                this.BuildReferencedFragmentList(node);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFragment" /> class.
        /// </summary>
        /// <param name="inlineFragmentNode">The inline fragment node.</param>
        public DocumentFragment(FragmentNode inlineFragmentNode)
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

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public FragmentNode Node { get; }

        /// <inheritdoc />
        public IGraphType GraphType { get; set; }

        /// <inheritdoc />
        public CharMemoryHashSet ReferencedNamedFragments { get; }

        /// <inheritdoc />
        public bool IsReferenced { get; private set; }

        /// <inheritdoc />
        public IEnumerable<IDocumentPart> Children => Enumerable.Empty<IDocumentPart>();

        /// <inheritdoc />
        public string TargetGraphTypeName { get; }
    }
}