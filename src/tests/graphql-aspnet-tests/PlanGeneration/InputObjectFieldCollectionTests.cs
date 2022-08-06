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
    public class InputObjectFieldCollectionTests
    {
        [Test]
        public void FindArgumentByStringName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var item = colllection.FindFieldByName("thename");
            Assert.IsNotNull(item);
        }

        [Test]
        public void FindArgumentByStringName_ForNonArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var item = colllection.FindFieldByName("notAnArgument");
            Assert.IsNull(item);
        }

        [Test]
        public void FindArgumentByMemoryName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var item = colllection.FindFieldByName(name);
            Assert.IsNotNull(item);
        }

        [Test]
        public void FindArgumentByMemoryName_ForNonArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();
            var othername = "theothername".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var item = colllection.FindFieldByName(othername);
            Assert.IsNull(item);
        }

        [Test]
        public void TryGetByStringName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var result = colllection.TryGetValue("thename", out var fieldOut);
            Assert.IsTrue(result);
            Assert.AreEqual(fieldPart.Object, fieldOut);
        }

        [Test]
        public void TryGetByStringName_ForNonExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var result = colllection.TryGetValue("theOthername", out var fieldOut);
            Assert.IsFalse(result);
            Assert.IsNull(fieldOut);
        }
    }
}