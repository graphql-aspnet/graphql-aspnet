﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing2
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Parsing;

    /// <summary>
    /// An abstract syntax tree containing <see cref="SynNode"/> elements
    /// that represent the parsed query document.
    /// </summary>
    [DebuggerDisplay("{RootNode}")]
    public readonly struct SynTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SynTree" /> struct.
        /// </summary>
        /// <param name="rootNode">The root node of the tree.</param>
        /// <param name="minBlockCapacity">The minmum number of node blocks
        /// that can be contained within this tree.</param>
        /// <returns>The newly created syntax tree.</returns>
        public static SynTree FromNode(SynNode rootNode, int minBlockCapacity = 4)
        {
            if (rootNode.Coordinates.BlockIndex >= 0
                || rootNode.Coordinates.BlockPosition >= 0
                || rootNode.Coordinates.ChildBlockIndex >= 0)
            {
                throw new InvalidOperationException("Cannot allocate a syntax tree " +
                    "from a node that declares an explicit coordinate location.");
            }

            if (minBlockCapacity < 4)
                minBlockCapacity = 4;

            var nodePool = ArrayPool<SynNode[]>.Shared.Rent(minBlockCapacity);
            return new SynTree(rootNode, nodePool, 0);
        }

        /// <summary>
        /// Clones this tree instance. Any supplied not supplied to this method
        /// are instead pulled from this instance.
        /// </summary>
        /// <param name="rootNode">The new root node.</param>
        /// <returns>SynTree.</returns>
        public SynTree Clone(SynNode rootNode)
        {
            return this.Clone(
                rootNode,
                this.NodePool,
                this.BlockLength);
        }

        /// <summary>
        /// Clones this tree instance.
        /// </summary>
        /// <param name="newBlockLength">New used length of the block.</param>
        /// <returns>SynTree.</returns>
        public SynTree Clone(int newBlockLength)
        {
            return this.Clone(
                this.RootNode,
                this.NodePool,
                newBlockLength);
        }

        /// <summary>
        /// Clones this tree instance. Any supplied not supplied to this method
        /// are instead pulled from this instance.
        /// </summary>
        /// <param name="rootNode">The new root node, if any.</param>
        /// <param name="blockLength">Length of the set of nodes in the pool.</param>
        /// <returns>SynTree.</returns>
        public SynTree Clone(
            SynNode? rootNode = null,
            int? blockLength = null)
        {
            return this.Clone(
                rootNode ?? this.RootNode,
                this.NodePool,
                blockLength ?? this.BlockLength);
        }

        /// <summary>
        /// Clones this tree instance. Any supplied not supplied to this method
        /// are instead pulled from this instance.
        /// </summary>
        /// <param name="rootNode">The new root node, if any.</param>
        /// <param name="nodePool">The new node pool, if any.</param>
        /// <param name="blockLength">Length of the set of nodes in the pool.</param>
        /// <returns>SynTree.</returns>
        public SynTree Clone(
            SynNode[][] nodePool,
            int blockLength)
        {
            Validation.ThrowIfNull(nodePool, nameof(nodePool));
            return this.Clone(this.RootNode, nodePool, blockLength);
        }

        /// <summary>
        /// Clones this tree instance. Any supplied not supplied to this method
        /// are instead pulled from this instance.
        /// </summary>
        /// <param name="rootNode">The new root node, if any.</param>
        /// <param name="nodePool">The new node pool, if any.</param>
        /// <param name="blockLength">Length of the set of nodes in the pool.</param>
        /// <returns>SynTree.</returns>
        private SynTree Clone(
            SynNode rootNode,
            SynNode[][] nodePool,
            int blockLength)
        {
            return new SynTree(rootNode, nodePool, blockLength);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SynTree" /> struct.
        /// </summary>
        /// <param name="rootNode">The root node of the tree.</param>
        /// <param name="nodePool">The node pool to hold all the nodes
        /// of this tree. The <paramref name="rootNode"/> should be in the
        /// pool at position 0.</param>
        /// <param name="blockLength">The length of used nodes in the <paramref name="nodePool"/>.</param>
        public SynTree(SynNode rootNode, SynNode[][] nodePool, int blockLength)
        {
            this.RootNode = rootNode;
            this.NodePool = nodePool;
            this.BlockLength = blockLength;
        }

        /// <summary>
        /// Gets the number of nodes consumed within the <see cref="NodePool"/>.
        /// </summary>
        /// <value>The length.</value>
        public int BlockLength { get; }

        /// <summary>
        /// Gets a jagged array representing the "sets of children" known to this
        /// tree. Each node of the tree contains a pointer to location in the pool
        /// where its children are located.
        /// </summary>
        /// <value>All nodes.</value>
        public SynNode[][] NodePool { get; }

        /// <summary>
        /// Gets the top level root document node of which all other nodes
        /// are decendents of.
        /// </summary>
        /// <value>The root node.</value>
        public SynNode RootNode { get; }
    }
}