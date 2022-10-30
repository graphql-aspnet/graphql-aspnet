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
    using NUnit.Framework;

    [TestFixture]
    public class SynTreeTests
    {
        [Test]
        public void AddSingleChild_ToTreeRoot_TreeIsExtended()
        {
            var rootNode = new SynNode(
                SynNodeType.Document,
                new SynNodeValue("Root0".AsMemory()));

            var childNode = new SynNode(
                SynNodeType.Operation,
                new SynNodeValue("Child1".AsMemory()));

            var tree = SynTree.FromNode(rootNode);
            var (updatedTree, updatedChildNode) = tree.AddChildNode(childNode);

            // one block representing the children of the root node should exist
            Assert.AreEqual(1, updatedTree.BlockLength);

            // child block index of the root node points to the right place in the pool
            Assert.AreEqual(0, updatedTree.RootNode.Coordinates.ChildBlockIndex);

            // child node exists at the right block position
            Assert.AreEqual(updatedChildNode.PrimaryValue, updatedTree.NodePool[0][0].PrimaryValue);

            // child node coordinates are set
            Assert.AreEqual(0, updatedChildNode.Coordinates.BlockIndex);
            Assert.AreEqual(0, updatedChildNode.Coordinates.BlockPosition);

            // no child block set for the child node
            Assert.AreEqual(-1, updatedChildNode.Coordinates.ChildBlockIndex);
            Assert.AreEqual(0, updatedChildNode.Coordinates.ChildBlockLength);

            updatedTree.Release();
        }
    }
}