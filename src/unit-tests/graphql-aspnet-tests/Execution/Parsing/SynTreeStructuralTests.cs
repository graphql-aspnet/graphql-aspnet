// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Parsing
{
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using NUnit.Framework;

    [TestFixture]
    internal class SynTreeStructuralTests
    {
        [Test]
        public void AddSingleChild_ToTreeRoot_TreeIsExtended_RootIsUpdated()
        {
            /* Creating this structure:
             *
             * Node-0
             * |
             *  - Node-0-0
             */
            var rootNode = new SyntaxNode(
                SyntaxNodeType.Document,
                new SyntaxNodeValue(new SourceTextBlockPointer(1, 1)));

            var childNode = new SyntaxNode(
                SyntaxNodeType.Operation,
                new SyntaxNodeValue(new SourceTextBlockPointer(2, 1)));

            var tree = SyntaxTree.FromNode(rootNode);
            SyntaxTreeOperations.AddChildNode(ref tree, ref childNode);

            // one block representing the children of the root node should exist
            Assert.AreEqual(1, tree.BlockLength);

            // child block index of the root node points to the right place in the pool
            Assert.AreEqual(0, tree.RootNode.Coordinates.ChildBlockIndex);

            // child node exists at the right block position
            Assert.IsTrue((bool)(childNode.PrimaryValue == tree.NodePool[0][0].PrimaryValue));

            // child node coordinates are set
            Assert.AreEqual(0, childNode.Coordinates.BlockIndex);
            Assert.AreEqual(0, childNode.Coordinates.BlockPosition);

            // no child block set for the child node
            Assert.AreEqual(-1, childNode.Coordinates.ChildBlockIndex);
            Assert.AreEqual(0, childNode.Coordinates.ChildBlockLength);

            // root node on the tree has correct child lengths
            Assert.AreEqual(0, tree.RootNode.Coordinates.ChildBlockIndex);
            Assert.AreEqual(1, tree.RootNode.Coordinates.ChildBlockLength);

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void AddSecondChild_ToTreeRoot_ChildrenAreAddedToSameBlock()
        {
            /* Creating this structure:
             *
             * Node-0
             * |
             *  - Node-0-0
             * |
             *  - Node-0-1
             */
            var rootNode = new SyntaxNode(
                SyntaxNodeType.Document,
                new SyntaxNodeValue(new SourceTextBlockPointer(1, 1)));

            var childNode0 = new SyntaxNode(
                SyntaxNodeType.Operation,
                new SyntaxNodeValue(new SourceTextBlockPointer(2, 1)));

            var childNode1 = new SyntaxNode(
                SyntaxNodeType.Operation,
                new SyntaxNodeValue(new SourceTextBlockPointer(3, 1)));

            var tree = SyntaxTree.FromNode(rootNode);
            SyntaxTreeOperations.AddChildNode(ref tree, ref childNode0);
            SyntaxTreeOperations.AddChildNode(ref tree, ref childNode1);

            // one block representing the children of the root node should exist
            Assert.AreEqual(1, tree.BlockLength);

            // child block index of the root node points to the expected place in the pool
            Assert.AreEqual(0, tree.RootNode.Coordinates.ChildBlockIndex);

            // child1 node exists at the right block position
            Assert.IsTrue((bool)(childNode1.PrimaryValue == tree.NodePool[0][1].PrimaryValue));

            // child1 node coordinates are set
            Assert.AreEqual(0, childNode1.Coordinates.BlockIndex);
            Assert.AreEqual(1, childNode1.Coordinates.BlockPosition);

            // no child block set for the child1 node
            Assert.AreEqual(-1, childNode1.Coordinates.ChildBlockIndex);
            Assert.AreEqual(0, childNode1.Coordinates.ChildBlockLength);

            // child0 coordinates are set
            Assert.AreEqual(0, childNode0.Coordinates.BlockIndex);
            Assert.AreEqual(0, childNode0.Coordinates.BlockPosition);

            // tree root shows two children
            Assert.AreEqual(0, tree.RootNode.Coordinates.ChildBlockIndex);
            Assert.AreEqual(2, tree.RootNode.Coordinates.ChildBlockLength);

            SyntaxTreeOperations.Release(ref tree);
        }

        [Test]
        public void AddChildOfChild_StructureIsUpdatedAppropriately()
        {
            /* Creating this structure:
             *
             * Node-0
             * |
             *  - Node-0-0
             *    |
             *    - Node-0-0-0
             */

            var rootNode = new SyntaxNode(
                SyntaxNodeType.Document,
                new SyntaxNodeValue(new SourceTextBlockPointer(1, 1)));

            var childNode0 = new SyntaxNode(
                SyntaxNodeType.Operation,
                new SyntaxNodeValue(new SourceTextBlockPointer(2, 1)));

            var childNode1 = new SyntaxNode(
                SyntaxNodeType.Operation,
                new SyntaxNodeValue(new SourceTextBlockPointer(3, 1)));

            var tree = SyntaxTree.FromNode(rootNode);

            // add child0 to root
            SyntaxTreeOperations.AddChildNode(ref tree, ref childNode0);

            // add child1 to child0
            SyntaxTreeOperations.AddChildNode(ref tree, ref childNode0, ref childNode1);

            // [0] = Children of Node-0
            // [1] = Children of Node-0-0
            Assert.AreEqual(2, tree.BlockLength);

            // check contents of Node-0 child block
            Assert.IsTrue((bool)(childNode0 == tree.NodePool[0][0]));

            // check contents of Node-0-0 child block
            Assert.IsTrue((bool)(childNode1 == tree.NodePool[1][0]));

            // check coords of child0
            Assert.AreEqual(0, childNode0.Coordinates.BlockIndex);
            Assert.AreEqual(0, childNode0.Coordinates.BlockPosition);
            Assert.AreEqual(1, childNode0.Coordinates.ChildBlockIndex);
            Assert.AreEqual(1, childNode0.Coordinates.ChildBlockLength);

            // check coords of child1
            Assert.AreEqual(1, childNode1.Coordinates.BlockIndex);
            Assert.AreEqual(0, childNode1.Coordinates.BlockPosition);
            Assert.AreEqual(-1, childNode1.Coordinates.ChildBlockIndex);
            Assert.AreEqual(0, childNode1.Coordinates.ChildBlockLength);

            SyntaxTreeOperations.Release(ref tree);
        }
    }
}