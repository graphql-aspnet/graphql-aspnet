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
    public class InlineFragmentNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InlineFragmentNode" /> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        /// <param name="targetType">The name of the graphql type this fragement targets or can be applied to.</param>
        public InlineFragmentNode(SourceLocation startLocation, ReadOnlyMemory<char> targetType)
            : base(startLocation)
        {
            this.TargetType = targetType;
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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"IF";
        }
    }
}