// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.SyntaxNodes.Fragments
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A node representing an inline fragment.
    /// </summary>
    [DebuggerDisplay("Target = {TargetType}")]
    public class FragmentNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FragmentNode" /> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        /// <param name="targetType">The name of the graphql type this fragement targets or can be applied to.</param>
        public FragmentNode(SourceLocation startLocation, ReadOnlyMemory<char> targetType)
            : base(startLocation)
        {
            this.TargetType = targetType;
        }

        /// <summary>
        /// Determines whether this instance can contain the child being added.
        /// </summary>
        /// <param name="childNode">The child node.</param>
        /// <returns><c>true</c> if this instance can have the node as a child; otherwise, <c>false</c>.</returns>
        protected override bool CanHaveChild(SyntaxNode childNode)
        {
            return childNode is FieldCollectionNode ||
                   childNode is DirectiveNode;
        }

        /// <summary>
        /// Gets the name of the graphql type this fragment applies to.
        /// </summary>
        /// <value>The type of the target.</value>
        public ReadOnlyMemory<char> TargetType { get; }

        /// <summary>
        /// Gets the friendly name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        protected override string NodeName => "Fragment";
    }
}