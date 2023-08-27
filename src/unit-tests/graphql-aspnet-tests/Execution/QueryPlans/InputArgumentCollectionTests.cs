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
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Schema;
    using NSubstitute;
    using NUnit.Framework;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;

    [TestFixture]
    public class InputArgumentCollectionTests
    {
        [Test]
        public void FindArgumentByStringName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename";

            var owner = Substitute.For<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner);

            var graphArg = Substitute.For<IGraphArgument>();
            graphArg.TypeExpression.Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Name.Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner,
                graphArg,
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

            var owner = Substitute.For<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner);

            var graphArg = Substitute.For<IGraphArgument>();
            graphArg.TypeExpression.Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Name.Returns("bob");

            var arg = new DocumentInputArgument(
                owner,
                graphArg,
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

            var owner = Substitute.For<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner);

            var graphArg = Substitute.For<IGraphArgument>();
            graphArg.TypeExpression.Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Name.Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner,
                graphArg,
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

            var owner = Substitute.For<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner);

            var graphArg = Substitute.For<IGraphArgument>();
            graphArg.TypeExpression.Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Name.Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner,
                graphArg,
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

            var owner = Substitute.For<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner);

            var graphArg = Substitute.For<IGraphArgument>();
            graphArg.TypeExpression.Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Name.Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner,
                graphArg,
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

            var owner = Substitute.For<IFieldDocumentPart>();
            var colllection = new DocumentInputArgumentCollection(owner);

            var graphArg = Substitute.For<IGraphArgument>();
            graphArg.TypeExpression.Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphArg.Name.Returns(name.ToString());

            var arg = new DocumentInputArgument(
                owner,
                graphArg,
                name,
                SourceLocation.None);

            colllection.AddArgument(arg);

            var result = colllection.TryGetValue("theOthername", out var argOut);
            Assert.IsFalse((bool)result);
            Assert.IsNull(argOut);
        }
    }
}