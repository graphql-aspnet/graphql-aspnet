// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.PlanGeneration
{
    using System;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.Schemas;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class QueryInputArgumentCollectionTests
    {
        [Test]
        public void FindArgumentByStringName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var colllection = new QueryInputArgumentCollection();

            var arg = new QueryInputArgument(
                new InputItemNode(SourceLocation.None, name),
                new Mock<IGraphType>().Object,
                new AspNet.Schemas.GraphTypeExpression("bob"));

            colllection.AddArgument(arg);

            var item = colllection.FindArgumentByName("thename");
            Assert.IsNotNull(item);
        }

        [Test]
        public void FindArgumentByStringName_ForNonArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var colllection = new QueryInputArgumentCollection();

            var arg = new QueryInputArgument(
                new InputItemNode(SourceLocation.None, name),
                new Mock<IGraphType>().Object,
                new AspNet.Schemas.GraphTypeExpression("bob"));

            colllection.AddArgument(arg);

            var item = colllection.FindArgumentByName("notAnArgument");
            Assert.IsNull(item);
        }

        [Test]
        public void FindArgumentByMemoryName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var colllection = new QueryInputArgumentCollection();

            var arg = new QueryInputArgument(
                new InputItemNode(SourceLocation.None, name),
                new Mock<IGraphType>().Object,
                new AspNet.Schemas.GraphTypeExpression("bob"));

            colllection.AddArgument(arg);

            var item = colllection.FindArgumentByName(name);
            Assert.IsNotNull(item);
        }

        [Test]
        public void FindArgumentByMemoryName_ForNonArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();
            var othername = "theothername".AsMemory();

            var colllection = new QueryInputArgumentCollection();

            var arg = new QueryInputArgument(
                new InputItemNode(SourceLocation.None, name),
                new Mock<IGraphType>().Object,
                new AspNet.Schemas.GraphTypeExpression("bob"));

            colllection.AddArgument(arg);

            var item = colllection.FindArgumentByName(othername);
            Assert.IsNull(item);
        }

        [Test]
        public void TryGetByStringName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var colllection = new QueryInputArgumentCollection();

            var arg = new QueryInputArgument(
                new InputItemNode(SourceLocation.None, name),
                new Mock<IGraphType>().Object,
                new AspNet.Schemas.GraphTypeExpression("bob"));

            colllection.AddArgument(arg);

            var result = colllection.TryGetValue("thename", out var argOut);
            Assert.IsTrue(result);
            Assert.AreEqual(arg, argOut);
        }

        [Test]
        public void TryGetByStringName_ForNonExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var colllection = new QueryInputArgumentCollection();

            var arg = new QueryInputArgument(
                new InputItemNode(SourceLocation.None, name),
                new Mock<IGraphType>().Object,
                new GraphTypeExpression("bob"));

            colllection.AddArgument(arg);

            var result = colllection.TryGetValue("theOthername", out var argOut);
            Assert.IsFalse(result);
            Assert.IsNull(argOut);
        }
    }
}