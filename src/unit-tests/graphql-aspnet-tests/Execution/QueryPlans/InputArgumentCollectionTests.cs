// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.QueryPlans
{
    using System;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Schema;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class InputArgumentCollectionTests
    {
        [Test]
        public void FindArgumentByStringName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename";

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner.Object);

            var graphArg = new Mock<IGraphArgument>();
            graphArg.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Setup(x => x.Name).Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner.Object,
                graphArg.Object,
                name,
                SourceLocation.None);

            colllection.AddArgument(arg);

            var item = colllection.FindArgumentByName(name);
            Assert.IsNotNull(item);
        }

        [Test]
        public void FindArgumentByStringName_ForNonArgument_ReturnsArgument()
        {
            var name = "thename";

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner.Object);

            var graphArg = new Mock<IGraphArgument>();
            graphArg.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Setup(x => x.Name).Returns("bob");

            var arg = new DocumentInputArgument(
                owner.Object,
                graphArg.Object,
                name,
                SourceLocation.None);

            colllection.AddArgument(arg);

            var item = colllection.FindArgumentByName("notAnArgument");
            Assert.IsNull(item);
        }

        [Test]
        public void FindArgumentByMemoryName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename";

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner.Object);

            var graphArg = new Mock<IGraphArgument>();
            graphArg.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Setup(x => x.Name).Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner.Object,
                graphArg.Object,
                name,
                SourceLocation.None);

            colllection.AddArgument(arg);

            var item = colllection.FindArgumentByName(name.AsMemory());
            Assert.IsNotNull(item);
        }

        [Test]
        public void FindArgumentByMemoryName_ForNonArgument_ReturnsNothing()
        {
            var name = "thename";
            var othername = "theothername";

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner.Object);

            var graphArg = new Mock<IGraphArgument>();
            graphArg.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Setup(x => x.Name).Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner.Object,
                graphArg.Object,
                name,
                SourceLocation.None);

            colllection.AddArgument(arg);

            var item = colllection.FindArgumentByName(othername.AsMemory());
            Assert.IsNull(item);
        }

        [Test]
        public void TryGetByStringName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename";

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner.Object);

            var graphArg = new Mock<IGraphArgument>();
            graphArg.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Setup(x => x.Name).Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner.Object,
                graphArg.Object,
                name,
                SourceLocation.None);

            colllection.AddArgument(arg);

            var result = colllection.TryGetValue("thename", out var argOut);
            Assert.IsTrue((bool)result);
            Assert.AreEqual(arg, argOut);
        }

        [Test]
        public void TryGetByStringName_ForNonExistingArgument_ReturnsArgument()
        {
            var name = "thename";

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner.Object);

            var graphArg = new Mock<IGraphArgument>();
            graphArg.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Setup(x => x.Name).Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner.Object,
                graphArg.Object,
                name,
                SourceLocation.None);

            colllection.AddArgument(arg);

            var result = colllection.TryGetValue("theOthername", out var argOut);
            Assert.IsFalse((bool)result);
            Assert.IsNull(argOut);
        }
    }
}