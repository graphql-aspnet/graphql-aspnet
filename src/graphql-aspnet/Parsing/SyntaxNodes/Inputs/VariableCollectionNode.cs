// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.SyntaxNodes.Inputs
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A contained collection of defined variables.
    /// </summary>
    /// <seealso cref="SyntaxNode" />
    [DebuggerDisplay("Count = {Children.Count})")]
    public class VariableCollectionNode : SyntaxNode, IEnumerable<VariableNode>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VariableCollectionNode"/> class.
        /// </summary>
        /// <param name="startLocation">The start location.</param>
        public VariableCollectionNode(SourceLocation startLocation)
            : base(startLocation)
        {
        }

        /// <summary>
        /// Determines whether this instance can contain the child being added.
        /// </summary>
        /// <param name="childNode">The child node.</param>
        /// <returns><c>true</c> if this instance can have the node as a child; otherwise, <c>false</c>.</returns>
        protected override bool CanHaveChild(SyntaxNode childNode)
        {
            return childNode is VariableNode;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<VariableNode> GetEnumerator()
        {
            return this.Children.Cast<VariableNode>().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}