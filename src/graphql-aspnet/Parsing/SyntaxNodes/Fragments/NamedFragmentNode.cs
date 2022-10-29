﻿// *************************************************************
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
    /// A formal graphql fragment declaration that can be pointed to by a <see cref="FragmentSpreadNode"/>.
    /// </summary>
    [DebuggerDisplay("Named Fragment {FragmentName}")]
    public class NamedFragmentNode : SyntaxNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedFragmentNode"/> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        /// <param name="name">The name.</param>
        /// <param name="targetType">Type of the target.</param>
        public NamedFragmentNode(
            SourceLocation startLocation,
            ReadOnlyMemory<char> name,
            ReadOnlyMemory<char> targetType)
            : base(startLocation)
        {
            this.FragmentName = name;
            this.TargetType = targetType;
        }

        /// <summary>
        /// Gets the name of this instance.
        /// </summary>
        /// <value>The name of the fragment.</value>
        public ReadOnlyMemory<char> FragmentName { get; }

        /// <summary>
        /// Gets the name of the graphql type this fragment applies to.
        /// </summary>
        /// <value>The type of the target.</value>
        public ReadOnlyMemory<char> TargetType { get; }

        /// <summary>
        /// Gets the friendly name of the node.
        /// </summary>
        /// <value>The name of the node.</value>
        protected override string NodeName => "NamedFragment";

        /// <inheritdoc />
        public override string ToString()
        {
            return $"NF-{this.FragmentName}";
        }
    }
}