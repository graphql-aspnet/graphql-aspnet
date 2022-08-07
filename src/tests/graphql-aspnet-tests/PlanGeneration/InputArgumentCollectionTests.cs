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
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing.SyntaxNodes.Inputs;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using GraphQL.AspNet.Schemas;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class InputArgumentCollectionTests
    {
        [Test]
        public void FindArgumentByStringName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner.Object);

            var graphArg = new Mock<IGraphArgument>();
            graphArg.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Setup(x => x.Name).Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner.Object,
                new InputItemNode(SourceLocation.None, name),
                graphArg.Object);

            colllection.AddArgument(arg);

            var item = colllection.FindArgumentByName("thename");
            Assert.IsNotNull(item);
        }

        [Test]
        public void FindArgumentByStringName_ForNonArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner.Object);

            var graphArg = new Mock<IGraphArgument>();
            graphArg.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Setup(x => x.Name).Returns("bob");

            var arg = new DocumentInputArgument(
                owner.Object,
                new InputItemNode(SourceLocation.None, name),
                graphArg.Object);

            colllection.AddArgument(arg);

            var item = colllection.FindArgumentByName("notAnArgument");
            Assert.IsNull(item);
        }

        [Test]
        public void FindArgumentByMemoryName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner.Object);

            var graphArg = new Mock<IGraphArgument>();
            graphArg.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Setup(x => x.Name).Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner.Object,
                new InputItemNode(SourceLocation.None, name),
                graphArg.Object);

            colllection.AddArgument(arg);

            var item = colllection.FindArgumentByName(name);
            Assert.IsNotNull(item);
        }

        [Test]
        public void FindArgumentByMemoryName_ForNonArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();
            var othername = "theothername".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner.Object);

            var graphArg = new Mock<IGraphArgument>();
            graphArg.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Setup(x => x.Name).Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner.Object,
                new InputItemNode(SourceLocation.None, name),
                graphArg.Object);

            colllection.AddArgument(arg);

            var item = colllection.FindArgumentByName(othername);
            Assert.IsNull(item);
        }

        [Test]
        public void TryGetByStringName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner.Object);

            var graphArg = new Mock<IGraphArgument>();
            graphArg.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Setup(x => x.Name).Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner.Object,
                new InputItemNode(SourceLocation.None, name),
                graphArg.Object);

            colllection.AddArgument(arg);

            var result = colllection.TryGetValue("thename", out var argOut);
            Assert.IsTrue(result);
            Assert.AreEqual(arg, argOut);
        }

        [Test]
        public void TryGetByStringName_ForNonExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner.Object);

            var graphArg = new Mock<IGraphArgument>();
            graphArg.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Setup(x => x.Name).Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner.Object,
                new InputItemNode(SourceLocation.None, name),
                graphArg.Object);

            colllection.AddArgument(arg);

            var result = colllection.TryGetValue("theOthername", out var argOut);
            Assert.IsFalse(result);
            Assert.IsNull(argOut);
        }
    }
}