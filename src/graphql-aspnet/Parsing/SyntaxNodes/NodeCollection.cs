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
    using System.Buffers;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// A collection of (potentially) unrelated syntax nodes.
    /// </summary>
    /// <seealso cref="List{SyntaxNode}" />
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public class NodeCollection : IReadOnlyList<SyntaxNode>, IDisposable
    {
        private SyntaxNode[] _nodes;
        private int _nextIndex;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeCollection" /> class.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        public NodeCollection(int capacity = 4)
        {

            // if a node collection is being made its because
            // a child WILL be added. Instead of an initital capcity
            // of zero that needs to reallocate, just set it
            _nodes = ArrayPool<SyntaxNode>.Shared.Rent(capacity);
            _nextIndex = 0;
        }

        /// <summary>
        /// Adds the specified item to the list.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(SyntaxNode item)
        {
            if (_nextIndex == _nodes.Length)
            {
                var expandedNodes = ArrayPool<SyntaxNode>.Shared.Rent(_nodes.Length * 2);

                for (var i = 0; i < _nextIndex; i++)
                {
                    expandedNodes[i] = _nodes[i];
                    _nodes[i] = null;
                }

                ArrayPool<SyntaxNode>.Shared.Return(_nodes);
                _nodes = expandedNodes;
            }

            _nodes[_nextIndex++] = item;
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

        /// <inheritdoc />
        public IEnumerator<SyntaxNode> GetEnumerator()
        {
            return ((IEnumerable<SyntaxNode>)new ArraySegment<SyntaxNode>(_nodes, 0, _nextIndex)).GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public SyntaxNode this[int index] => _nodes[index];

        /// <inheritdoc />
        public int Count => _nextIndex;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    for (var i = 0; i < _nextIndex; i++)
                    {
                        _nodes[i].Dispose();
                        _nodes[i] = null;
                    }

                    ArrayPool<SyntaxNode>.Shared.Return(_nodes);
                }

                _isDisposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}