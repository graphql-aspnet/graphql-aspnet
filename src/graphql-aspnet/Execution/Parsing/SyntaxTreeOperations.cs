// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing
{
    using System;
    using System.Buffers;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;

    /// <summary>
    /// Extension methods for <see cref="SyntaxTree"/>.
    /// </summary>
    public static class SyntaxTreeOperations
    {
        /// <summary>
        /// Releases the pool of nodes within the syntax tree. Once released,
        /// the tree should be discarded.
        /// </summary>
        /// <param name="synTree">The syn tree to release.</param>
        [DebuggerStepperBoundary]
        public static void Release(ref SyntaxTree synTree)
        {
            for (var i = 0; i < synTree.BlockLength; i++)
                ArrayPool<SyntaxNode>.Shared.Return(synTree.NodePool[i]);

            ArrayPool<SyntaxNode[]>.Shared.Return(synTree.NodePool);
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
        [DebuggerStepperBoundary]
        public static void AddChildNode(
            ref SyntaxTree synTree,
            ref SyntaxNode childNode)
        {
            var rootNode = synTree.RootNode;
            AddChildNode(ref synTree, ref rootNode, ref childNode);
        }

        /// <summary>
        /// Adds the child node to the syntax tree as a child of <paramref name="parentNode"/>.
        /// It is assumed that <paramref name="parentNode"/> is already on the supplied <see cref="SyntaxTree"/>.
        /// </summary>
        /// <remarks>
        /// This is a destructive process. the original syntax tree is consumed
        /// during hte update.
        /// </remarks>
        /// <param name="synTree">The syn tree to add the child to.</param>
        /// <param name="parentNode">The parent node to add the child to.</param>
        /// <param name="childNode">The child node to insert.</param>
        [DebuggerStepperBoundary]
        public static void AddChildNode(
            ref SyntaxTree synTree,
            ref SyntaxNode parentNode,
            ref SyntaxNode childNode)
        {
            // add or update the parent's child block where the child
            // will be added
            if (parentNode.Coordinates.ChildBlockIndex < 0)
            {
                InsertChildBlock(ref synTree, ref parentNode);
            }
            else
            {
                EnsureExistingChildBlockLength(
                    ref synTree,
                    ref parentNode,
                    parentNode.Coordinates.ChildBlockLength + 1);
            }

            // fix the coordinates of the child block
            childNode = childNode.Clone(
                coords: new SyntaxNodeCoordinates(
                    parentNode.Coordinates.ChildBlockIndex,
                    parentNode.Coordinates.ChildBlockLength));

            // insert the child block into the map
            var coords = childNode.Coordinates;
            synTree.NodePool[coords.BlockIndex][coords.BlockPosition] = childNode;

            // update the parent's child count
            coords = parentNode.Coordinates;
            parentNode = parentNode.Clone(coords: coords.Clone(childBlockLength: coords.ChildBlockLength + 1));

            // ensure the parentNode is updated within the
            // tree copy to reflect its new child stats
            if (parentNode.IsRootNode)
            {
                synTree = synTree.Clone(rootNode: parentNode);
            }
            else
            {
                coords = parentNode.Coordinates;
                synTree.NodePool[coords.BlockIndex][coords.BlockPosition] = parentNode;
            }
        }

        /// <summary>
        /// Inserts a node block for the children of the provided node.
        /// </summary>
        /// <param name="synTree">The syntax tree to add the block to.</param>
        /// <param name="node">The node to add the block for.</param>
        private static void InsertChildBlock(ref SyntaxTree synTree, ref SyntaxNode node)
        {
            // create a new block to hold the children
            var newBlock = ArrayPool<SyntaxNode>.Shared.Rent(4);

            // expand the size of the block pool if needed
            if (synTree.BlockLength + 1 > synTree.NodePool.Length)
            {
                var oldPool = synTree.NodePool;
                var newNodePool = ArrayPool<SyntaxNode[]>.Shared.Rent(oldPool.Length * 2);

                Array.Copy(oldPool, newNodePool, synTree.BlockLength);
                ArrayPool<SyntaxNode[]>.Shared.Return(oldPool);

                synTree = synTree.Clone(newNodePool, synTree.BlockLength);
            }

            // afix the new block into the tree
            var childBlockIndex = synTree.BlockLength;
            synTree.NodePool[childBlockIndex] = newBlock;
            synTree = synTree.Clone(synTree.BlockLength + 1);

            // update the node with the correct child block index
            node = node.Clone(
                coords: node
                    .Coordinates.Clone(childBlockIndex: childBlockIndex));
        }

        private static void EnsureExistingChildBlockLength(
            ref SyntaxTree synTree,
            ref SyntaxNode node,
            int minimumRequiredLength)
        {
            var coords = node.Coordinates;
            var childNodeBlock = synTree.NodePool[coords.ChildBlockIndex];
            if (childNodeBlock.Length >= minimumRequiredLength)
                return;

            // expand the tree to a new minLength
            var newChildBlockLength = childNodeBlock.Length * 2;
            while (newChildBlockLength < minimumRequiredLength)
                newChildBlockLength = newChildBlockLength * 2;

            var newChildBlock = ArrayPool<SyntaxNode>.Shared.Rent(newChildBlockLength);
            Array.Copy(childNodeBlock, newChildBlock, coords.ChildBlockLength);
            ArrayPool<SyntaxNode>.Shared.Return(childNodeBlock);

            synTree.NodePool[coords.ChildBlockIndex] = newChildBlock;
        }

        /// <summary>
        /// Prints out the syntax tree into a structured json document.
        /// </summary>
        /// <param name="synTree">The syntax tree to convert.</param>
        /// <param name="sourceText">The source text containing the various
        /// blocks of text referenced by the syntax tree.</param>
        /// <returns>System.String.</returns>
        public static string ToJsonString(this SyntaxTree synTree, SourceText sourceText)
        {
            var options = new JsonWriterOptions()
            {
                Indented = true,
            };

            var stream = new MemoryStream();
            var writer = new Utf8JsonWriter(stream, options);

            PrintJsonNode(writer, sourceText, synTree, synTree.RootNode, 0);
            writer.Flush();

            stream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var str = reader.ReadToEnd();

            reader.Dispose();
            writer.Dispose();
            stream.Dispose();

            return str;
        }

        private static void PrintJsonNode(Utf8JsonWriter writer, SourceText sourceText, SyntaxTree synTree, SyntaxNode node, int depth)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("depth");
            writer.WriteNumberValue(depth);

            writer.WritePropertyName("type");
            writer.WriteStringValue(node.NodeType.ToString());

            if (node.PrimaryValue != default)
            {
                writer.WritePropertyName("primary");
                var textValue = sourceText.Slice(node.PrimaryValue.TextBlock);
                writer.WriteStringValue(textValue.ToString());
            }

            if (node.SecondaryValue != default)
            {
                writer.WritePropertyName("secondary");
                var textValue = sourceText.Slice(node.SecondaryValue.TextBlock);
                writer.WriteStringValue(textValue.ToString());
            }

            if (node.Coordinates.ChildBlockIndex >= 0)
            {
                writer.WritePropertyName("children");
                writer.WriteStartArray();
                var childBlock = synTree.NodePool[node.Coordinates.ChildBlockIndex];
                for (var i = 0; i < node.Coordinates.ChildBlockLength; i++)
                    PrintJsonNode(writer, sourceText, synTree, childBlock[i], depth + 1);

                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}