// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Parsing2
{
    using System;
    using GraphQL.AspNet.Parsing;
    using GraphQL.AspNet.Parsing2;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class SynTreeTests
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
            var rootNode = new SynNode(
                SynNodeType.Document,
                new SynNodeValue("Node-0".AsMemory()));

            var childNode = new SynNode(
                SynNodeType.Operation,
                new SynNodeValue("Node-0-0".AsMemory()));

            var tree = SynTree.FromNode(rootNode);
            var updatedTree = tree.AddChildNode(ref childNode);

            // one block representing the children of the root node should exist
            Assert.AreEqual(1, updatedTree.BlockLength);

            // child block index of the root node points to the right place in the pool
            Assert.AreEqual(0, updatedTree.RootNode.Coordinates.ChildBlockIndex);

            // child node exists at the right block position
            Assert.AreEqual(childNode.PrimaryValue, updatedTree.NodePool[0][0].PrimaryValue);

            // child node coordinates are set
            Assert.AreEqual(0, childNode.Coordinates.BlockIndex);
            Assert.AreEqual(0, childNode.Coordinates.BlockPosition);

            // no child block set for the child node
            Assert.AreEqual(-1, childNode.Coordinates.ChildBlockIndex);
            Assert.AreEqual(0, childNode.Coordinates.ChildBlockLength);

            // root node on the tree has correct child lengths
            Assert.AreEqual(0, updatedTree.RootNode.Coordinates.ChildBlockIndex);
            Assert.AreEqual(1, updatedTree.RootNode.Coordinates.ChildBlockLength);

            updatedTree.Release();
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
            var rootNode = new SynNode(
                SynNodeType.Document,
                new SynNodeValue("Node-0".AsMemory()));

            var childNode0 = new SynNode(
                SynNodeType.Operation,
                new SynNodeValue("Node-0-0".AsMemory()));

            var childNode1 = new SynNode(
                SynNodeType.Operation,
                new SynNodeValue("Node-0-1".AsMemory()));

            var tree = SynTree.FromNode(rootNode);
            var updatedTree = tree.AddChildNode(ref childNode0);
            updatedTree = updatedTree.AddChildNode(ref childNode1);

            // one block representing the children of the root node should exist
            Assert.AreEqual(1, updatedTree.BlockLength);

            // child block index of the root node points to the expected place in the pool
            Assert.AreEqual(0, updatedTree.RootNode.Coordinates.ChildBlockIndex);

            // child1 node exists at the right block position
            Assert.AreEqual(childNode1.PrimaryValue, updatedTree.NodePool[0][1].PrimaryValue);

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
            Assert.AreEqual(0, updatedTree.RootNode.Coordinates.ChildBlockIndex);
            Assert.AreEqual(2, updatedTree.RootNode.Coordinates.ChildBlockLength);

            updatedTree.Release();
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

            var rootNode = new SynNode(
                SynNodeType.Document,
                new SynNodeValue("Node-0".AsMemory()));

            var childNode0 = new SynNode(
                SynNodeType.Operation,
                new SynNodeValue("Node-0-0".AsMemory()));

            var childNode1 = new SynNode(
                SynNodeType.Operation,
                new SynNodeValue("Node-0-0-0".AsMemory()));

            var tree = SynTree.FromNode(rootNode);

            // add child0 to root
            var updatedTree = tree.AddChildNode(ref childNode0);

            // add child1 to child0
            updatedTree = updatedTree.AddChildNode(ref childNode0, ref childNode1);

            // [0] = Children of Node-0
            // [1] = Children of Node-0-0
            Assert.AreEqual(2, updatedTree.BlockLength);

            // check contents of Node-0 child block
            Assert.AreEqual(childNode0, updatedTree.NodePool[0][0]);
            Assert.AreEqual(default(SynNode), updatedTree.NodePool[0][1]);

            // check contents of Node-0-0 child block
            Assert.AreEqual(childNode1, updatedTree.NodePool[1][0]);
            Assert.AreEqual(default(SynNode), updatedTree.NodePool[1][1]);

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
        }

        [Test]
        public void SynTree_JsonCompare()
        {
            var rootNode = new SynNode(
                SynNodeType.Document,
                new SynNodeValue("Node-0".AsMemory()));

            var childNode0 = new SynNode(
                SynNodeType.Operation,
                new SynNodeValue("Node-0-0".AsMemory()),
                new SynNodeValue("Node-0-0-secondary".AsMemory()));

            var childNode1 = new SynNode(
                SynNodeType.Field,
                new SynNodeValue("Node-0-0-0".AsMemory()));

            var childNode2 = new SynNode(
               SynNodeType.Field,
               new SynNodeValue("Node-0-0-1".AsMemory()));

            var childNode3 = new SynNode(
              SynNodeType.NamedFragment,
              new SynNodeValue("Node-0-1".AsMemory()));

            var childNode4 = new SynNode(
                SynNodeType.Field,
                new SynNodeValue("Node-0-0-0-0".AsMemory()));

            var childNode5 = new SynNode(
                SynNodeType.Field,
                new SynNodeValue("Node-0-0-0-1".AsMemory()));

            var tree = SynTree.FromNode(rootNode);
            tree = tree.AddChildNode(ref childNode0);
            tree = tree.AddChildNode(ref childNode0, ref childNode1);
            tree = tree.AddChildNode(ref childNode0, ref childNode2);
            tree = tree.AddChildNode(ref childNode1, ref childNode4);
            tree = tree.AddChildNode(ref childNode1, ref childNode5);
            tree = tree.AddChildNode(ref childNode3);

            var expected = @"
            {
                ""depth"": 0,
                ""type"": ""Document"",
                ""primary"": ""Node-0"",
                ""children"": [
                    {
                        ""depth"": 1,
                        ""type"": ""Operation"",
                        ""primary"": ""Node-0-0"",
                        ""secondary"": ""Node-0-0-secondary"",
                        ""children"": [
                            {
                                ""depth"": 2,
                                ""type"": ""Field"",
                                ""primary"": ""Node-0-0-0"",
                                ""children"": [
                                    {
                                        ""depth"": 3,
                                        ""type"": ""Field"",
                                        ""primary"": ""Node-0-0-0-0""
                                    },
                                    {
                                        ""depth"": 3,
                                        ""type"": ""Field"",
                                        ""primary"": ""Node-0-0-0-1""
                                    }
                                ]
                            },
                            {
                                ""depth"": 2,
                                ""type"": ""Field"",
                                ""primary"": ""Node-0-0-1""
                            }
                        ]
                    },
                    {
                        ""depth"": 1,
                        ""type"": ""NamedFragment"",
                        ""primary"": ""Node-0-1""
                    }
                ]
            }";

            var actual = tree.ToJsonString();
            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }
    }
}