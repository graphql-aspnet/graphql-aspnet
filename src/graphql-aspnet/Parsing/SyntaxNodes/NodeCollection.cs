// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.SyntaxNodes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// A collection of (potentially) unrelated syntax nodes.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.List{SyntaxNode}" />
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public class NodeCollection : List<SyntaxNode>
    {
        /// <summary>
        /// Determines if any node in this collection is of the provided type.
        /// </summary>
        /// <typeparam name="TChildNodeType">The type of the child node to search for.</typeparam>
        /// <returns><c>true</c> if at least one node matches the given type, <c>false</c> otherwise.</returns>
        public bool Any<TChildNodeType>()
            where TChildNodeType : SyntaxNode
        {
            return this.Any(x => x is TChildNodeType);
        }

        /// <summary>
        /// Finds the first child node of the given type or returns null if no
        /// matching children are found.
        /// </summary>
        /// <typeparam name="TChildNodeType">The type of the child node type.</typeparam>
        /// <returns>TChildNodeType.</returns>
        public TChildNodeType FirstOrDefault<TChildNodeType>()
            where TChildNodeType : SyntaxNode
        {
            return this.FirstOrDefault(x => x is TChildNodeType) as TChildNodeType;
        }

        /// <summary>
        /// Finds a single child node of the given type or returns null if no children or more than one
        /// child are found.
        /// </summary>
        /// <typeparam name="TChildNodeType">The type of the child node type.</typeparam>
        /// <returns>TChildNodeType.</returns>
        public TChildNodeType SingleOrDefault<TChildNodeType>()
            where TChildNodeType : SyntaxNode
        {
            return this.SingleOrDefault(x => x is TChildNodeType) as TChildNodeType;
        }
    }
}