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
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Parsing.SyntaxNodes;

    /// <summary>
    /// A concrete implemnetation of a the syntax tree to contain operations and fragments
    /// recieved on the text based query.
    /// </summary>
    /// <seealso cref="ISyntaxTree" />
    [DebuggerDisplay("Default Syntax Tree")]
    public class SyntaxTree : ISyntaxTree
    {
        private int _nextTempCollectionId;
        private int _nextIndex;
        private SyntaxNode[] _allNodes;
        private Dictionary<int, (SyntaxNode[] Nodes, int NextIndex)> _tempNodes;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxTree"/> class.
        /// </summary>
        public SyntaxTree()
        {
            this.RootNode = new DocumentNode();
            _tempNodes = new Dictionary<int, (SyntaxNode[] Nodes, int NextIndex)>(8);
            _allNodes = ArrayPool<SyntaxNode>.Shared.Rent(4);
            _allNodes[0] = this.RootNode;
            _nextIndex = 1;
            _nextTempCollectionId = 0;
        }

        /// <inheritdoc />
        public int BeginTempCollection()
        {
            var id = _nextTempCollectionId++;
            _tempNodes.Add(id, (ArrayPool<SyntaxNode>.Shared.Rent(1), 0));

            return id;
        }

        /// <inheritdoc />
        public void AddNodeToTempCollection(int collectionId, SyntaxNode node)
        {
            if (node == null)
                return;

            if (!_tempNodes.ContainsKey(collectionId))
                throw new KeyNotFoundException($"A temp collection with id {collectionId} does not exist");

            var collection = _tempNodes[collectionId];
            if (collection.Nodes.Length - 1 < collection.NextIndex)
            {
                var newCollection = this.ExpandCopyAndClear(
                    collection.Nodes,
                    collection.NextIndex);

                ArrayPool<SyntaxNode>.Shared.Return(collection.Nodes);
                collection.Nodes = newCollection;
            }

            collection.Nodes[collection.NextIndex] = node;

            _tempNodes[collectionId] = (collection.Nodes, collection.NextIndex + 1);
        }

        private SyntaxNode[] ExpandCopyAndClear(
            SyntaxNode[] nodeSet,
            int currentAllocatedLength,
            int? newTotalLength = null)
        {
            newTotalLength = newTotalLength ?? nodeSet.Length * 2;
            if (newTotalLength < nodeSet.Length)
                newTotalLength = nodeSet.Length + 1;

            var newNodes = ArrayPool<SyntaxNode>.Shared.Rent(newTotalLength.Value);
            for (var i = 0; i < currentAllocatedLength; i++)
            {
                newNodes[i] = nodeSet[i];
                nodeSet[i] = null;
            }

            return newNodes;
        }

        /// <inheritdoc />
        public (int StartIndex, int Length) PreserveCollection(int collectionId)
        {
            if (collectionId < 0)
                return (-1, 0);

            if (!_tempNodes.ContainsKey(collectionId))
                throw new KeyNotFoundException($"A temp collection with id {collectionId} does not exist");

            var collection = _tempNodes[collectionId];
            var collectionLength = collection.NextIndex;
            var startIndex = _nextIndex;

            // allocate more space in the all nodes array if necessary
            if (_nextIndex + collectionLength > _allNodes.Length)
            {
                var newLength = _allNodes.Length * 2;
                while (newLength < _nextIndex + collection.NextIndex)
                    newLength = newLength * 2;

                var newChildList = this.ExpandCopyAndClear(_allNodes, _nextIndex, newLength);
                ArrayPool<SyntaxNode>.Shared.Return(_allNodes);
                _allNodes = newChildList;
            }

            for (var i = 0; i < collectionLength; i++)
                _allNodes[_nextIndex++] = collection.Nodes[i];

            ArrayPool<SyntaxNode>.Shared.Return(collection.Nodes);
            _tempNodes.Remove(collectionId);

            if (collectionLength == 0)
                startIndex = -1;

            return (startIndex, collectionLength);
        }

        /// <inheritdoc />
        public int PreserveNode(SyntaxNode syntaxNode)
        {
            Validation.ThrowIfNull(syntaxNode, nameof(syntaxNode));

            if (_nextIndex >= _allNodes.Length)
            {
                var newLength = _allNodes.Length * 2;
                var newChildList = this.ExpandCopyAndClear(_allNodes, _nextIndex, newLength);
                ArrayPool<SyntaxNode>.Shared.Return(_allNodes);
                _allNodes = newChildList;
            }

            var indexOf = _nextIndex++;
            _allNodes[indexOf] = syntaxNode;
            return indexOf;
        }

        /// <inheritdoc />
        public ArraySegment<SyntaxNode> CreateSegment(int startIndex, int length)
        {
            if (startIndex < 0 || length <= 0)
                return new ArraySegment<SyntaxNode>(this.AllNodes, 0, 0);
            return new ArraySegment<SyntaxNode>(this.AllNodes, startIndex, length);
        }

        /// <inheritdoc />
        public DocumentNode RootNode { get; }

        /// <inheritdoc />
        public SyntaxNode[] AllNodes => _allNodes;

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
                    // null the array before returning it
                    // to prevent a lingering reference to the nodes
                    foreach (var kvp in _tempNodes)
                        ArrayPool<SyntaxNode>.Shared.Return(kvp.Value.Nodes);

                    ArrayPool<SyntaxNode>.Shared.Return(_allNodes);
                    _tempNodes.Clear();
                }

                _isDisposed = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}