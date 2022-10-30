// *************************************************************
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
    using GraphQL.AspNet.Parsing;

    /// <summary>
    /// Extension methods for <see cref="SynTree"/>.
    /// </summary>
    public static class SynTreeOperations
    {
        /// <summary>
        /// Releases the pool of nodes within the syntax tree. Once released,
        /// the tree should be discarded.
        /// </summary>
        /// <param name="synTree">The syn tree to release.</param>
        public static void Release(this SynTree synTree)
        {
            for (var i = 0; i < synTree.BlockLength; i++)
                ArrayPool<SynNode>.Shared.Return(synTree.NodePool[i]);

            ArrayPool<SynNode[]>.Shared.Return(synTree.NodePool);
        }

        /// <summary>
        /// Adds the single node as a child node to the syntax tree. This child
        /// is added as a direct child of the root node of the tree.
        /// </summary>
        /// <remarks>
        /// This is a destructive process. the original syntax tree is consumed
        /// during hte update.
        /// </remarks>
        /// <param name="synTree">The syn tree to add the child to.</param>
        /// <param name="childNode">The child node to insert.</param>
        /// <returns>A copy of the tree with the node inserted and an updated
        /// version of the node with the appropriate coordinates.</returns>
        public static (SynTree SyntaxTree, SynNode childNode) AddChildNode(this SynTree synTree, SynNode childNode)
        {
            var (treeOut, _, childOut) = AddChildNode(synTree, synTree.RootNode, childNode);
            return (treeOut, childOut);
        }

        /// <summary>
        /// Adds the child node to the syntax tree as a child of <paramref name="parentNode"/>.
        /// It is assumed that <paramref name="parentNode"/> is already on the supplied <see cref="SynTree"/>.
        /// </summary>
        /// <remarks>
        /// This is a destructive process. the original syntax tree is consumed
        /// during hte update.
        /// </remarks>
        /// <param name="synTree">The syn tree to add the child to.</param>
        /// <param name="parentNode">The parent node to add the child to.</param>
        /// <param name="childNode">The child node to insert.</param>
        /// <returns>A copy of the tree with the node inserted and an updated
        /// version of the node with the appropriate coordinates.</returns>
        public static (SynTree SyntaxTree, SynNode parentNode, SynNode childNode) AddChildNode(
            this SynTree synTree,
            SynNode parentNode,
            SynNode childNode)
        {
            SynTree treeOut;
            SynNode parentOut;
            SynNode childOut;

            if (parentNode.Coordinates.ChildBlockIndex < 0)
            {
                (treeOut, parentOut) = InsertChildBlock(synTree, parentNode);
            }
            else
            {
                (treeOut, parentOut) = EnsureExistingChildBlockLength(
                    synTree,
                    parentNode,
                    parentNode.Coordinates.ChildBlockLength + 1);
            }

            childOut = childNode.Clone(
                coords: new SynNodeCoordinates(
                    parentOut.Coordinates.ChildBlockIndex,
                    parentOut.Coordinates.ChildBlockLength));

            treeOut.NodePool
                [childOut.Coordinates.BlockIndex]
                [childOut.Coordinates.BlockPosition] = childOut;

            return (treeOut, parentOut, childOut);
        }

        /// <summary>
        /// Inserts a node block for the children of the provided node.
        /// </summary>
        /// <param name="synTree">The syntax tree to add the block to.</param>
        /// <param name="node">The node to add the block for.</param>
        /// <returns>The updated tree containing the block and the updated referncing the
        /// newly added block.</returns>
        private static (SynTree SyntaxTree, SynNode Node) InsertChildBlock(SynTree synTree, SynNode node)
        {
            SynTree treeOut = synTree;
            SynNode nodeOut = node;

            // add a block to the node pool to contain the child nodes
            // of the parent
            var newBlock = ArrayPool<SynNode>.Shared.Rent(4);
            if (synTree.BlockLength + 1 > synTree.NodePool.Length)
            {
                // expand the size of the block pool if needed
                var oldPool = treeOut.NodePool;
                var newNodePool = ArrayPool<SynNode[]>.Shared.Rent(oldPool.Length * 2);

                Array.Copy(oldPool, newNodePool, treeOut.BlockLength);
                ArrayPool<SynNode[]>.Shared.Return(oldPool);

                treeOut = treeOut.Clone(nodePool: newNodePool);
            }

            var childBlockIndex = treeOut.BlockLength;
            treeOut.NodePool[childBlockIndex] = newBlock;
            treeOut = treeOut.Clone(blockLength: treeOut.BlockLength + 1);

            // update the node with the correct child block index
            nodeOut = nodeOut.Clone(
                coords: nodeOut
                    .Coordinates.Clone(childBlockIndex: childBlockIndex));

            if (nodeOut.Coordinates.BlockIndex == -1)
            {
                // re-root the tree with the root node's new details
                treeOut = treeOut.Clone(rootNode: nodeOut);
            }
            else
            {
                // update the node in the tree with new details
                treeOut.NodePool
                    [nodeOut.Coordinates.BlockIndex]
                    [nodeOut.Coordinates.BlockPosition] = nodeOut;
            }

            return (treeOut, nodeOut);
        }

        private static (SynTree SyntaxTree, SynNode Node) EnsureExistingChildBlockLength(
            SynTree synTree,
            SynNode node,
            int minimumRequiredLength)
        {
            var treeOut = synTree;
            var nodeOut = node;

            var coords = node.Coordinates;
            var childNodeBlock = treeOut.NodePool[coords.ChildBlockIndex];
            if (childNodeBlock.Length >= minimumRequiredLength)
                return (treeOut, nodeOut);

            // expand the tree to a new minLength
            var newChildBlockLength = childNodeBlock.Length * 2;
            while (newChildBlockLength < minimumRequiredLength)
                newChildBlockLength = newChildBlockLength * 2;

            var newChildBlock = ArrayPool<SynNode>.Shared.Rent(newChildBlockLength);
            Array.Copy(childNodeBlock, newChildBlock, coords.ChildBlockLength);
            ArrayPool<SynNode>.Shared.Return(childNodeBlock);

            treeOut.NodePool[coords.ChildBlockIndex] = newChildBlock;
            return (treeOut, node);
        }
    }
}