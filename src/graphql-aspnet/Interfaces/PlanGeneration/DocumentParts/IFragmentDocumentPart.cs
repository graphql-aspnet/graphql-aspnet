// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts
{
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Fragments;

    /// <summary>
    /// A definition of a single fragment within the query document. Represents
    /// both a named fragment and an inline fragment.
    /// </summary>
    public interface IFragmentDocumentPart : IDirectiveContainerDocumentPart, IDocumentPart
    {
        /// <summary>
        /// Marks this fragment as being referenced and used in at least one operation in the document.
        /// </summary>
        internal void MarkAsReferenced();

        /// <summary>
        /// Gets the unique name of this reference in the collection.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the node this reference points to.
        /// </summary>
        /// <value>The node.</value>
        FragmentNode Node { get; }

        /// <summary>
        /// Gets or sets the graphtype this named fragment is restricted to when set. If null, there are no restrictions to
        /// where this named fragment can be used.
        /// </summary>
        /// <value>The type of the graph.</value>
        IGraphType GraphType { get; set; }

        /// <summary>
        /// Gets a collection of named fragments to be spread within this fragment.
        /// </summary>
        /// <value>The set of named fragments referenced by this instance.</value>
        CharMemoryHashSet ReferencedNamedFragments { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is referenced by an operation in the query document.
        /// </summary>
        /// <value><c>true</c> if this instance is referenced; otherwise, <c>false</c>.</value>
        bool IsReferenced { get; }

        /// <summary>
        /// Gets the name of the target graph type if any.
        /// </summary>
        /// <value>The name of the target graph type.</value>
        string TargetGraphTypeName { get; }
    }
}