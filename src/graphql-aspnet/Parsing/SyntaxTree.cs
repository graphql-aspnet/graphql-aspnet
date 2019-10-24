// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A concrete implemnetation of a the syntax tree to contain operations and fragments
    /// recieved on the text based query.
    /// </summary>
    /// <seealso cref="ISyntaxTree" />
    [DebuggerDisplay("Nodes = {Nodes.Count}")]
    public class SyntaxTree : ISyntaxTree
    {
        private readonly List<SyntaxNode> _nodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxTree"/> class.
        /// </summary>
        public SyntaxTree()
        {
            _nodes = new List<SyntaxNode>();
        }

        /// <summary>
        /// Gets a collection of the named operations contained in this document.
        /// </summary>
        /// <value>The dictionary, keyed on operation name, of the operations on the recieved query.</value>
        public IReadOnlyList<SyntaxNode> Nodes => _nodes;

        /// <summary>
        /// Adds the node to the syntax tree.
        /// </summary>
        /// <param name="node">The node.</param>
        internal void AddNode(SyntaxNode node)
        {
            _nodes.Add(node);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<SyntaxNode> GetEnumerator()
        {
            return _nodes.GetEnumerator();
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