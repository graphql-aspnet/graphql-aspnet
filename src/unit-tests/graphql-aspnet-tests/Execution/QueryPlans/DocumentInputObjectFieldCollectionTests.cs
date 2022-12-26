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
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Schema;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DocumentInputObjectFieldCollectionTests
    {
        [Test]
        public void FindArgumentByStringName_ForExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IInputGraphField>();
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

            var graphField = new Mock<IInputGraphField>();
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

            var graphField = new Mock<IInputGraphField>();
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

            var graphField = new Mock<IInputGraphField>();
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

            var graphField = new Mock<IInputGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var result = colllection.TryGetValue("thename", out var fieldOut);
            Assert.IsTrue((bool)result);
            Assert.AreEqual(fieldPart.Object, fieldOut);
        }

        [Test]
        public void TryGetByStringName_ForNonExistingArgument_ReturnsArgument()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IInputGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var result = colllection.TryGetValue("theOthername", out var fieldOut);
            Assert.IsFalse((bool)result);
            Assert.IsNull(fieldOut);
        }

        [Test]
        public void ContainsKey_ForNonArgument_ReturnsFalse()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IInputGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var item = colllection.ContainsKey("notAnArgument");
            Assert.IsFalse((bool)item);
        }

        [Test]
        public void ContainsKey_ForArgument_ReturnsTrue()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IInputGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var item = colllection.ContainsKey("thename");
            Assert.IsTrue((bool)item);
        }

        [Test]
        public void ContainsKeyByMemory_ForNonArgument_ReturnsFalse()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IInputGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var item = colllection.ContainsKey("notAnArgument".AsMemory());
            Assert.IsFalse((bool)item);
        }

        [Test]
        public void ContainsKeyByMemory_ForArgument_ReturnsTrue()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IInputGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var item = colllection.ContainsKey(name);
            Assert.IsTrue((bool)item);
        }

        [Test]
        public void IsUniqueByString_ForUniqueArgument_ReturnsTrue()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IInputGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var item = colllection.IsUnique("thename");
            Assert.IsTrue((bool)item);
        }

        [Test]
        public void IsUniqueByString_ForNullStringValue_ReturnsFalse()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IInputGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var item = colllection.IsUnique(null);
            Assert.IsFalse((bool)item);
        }

        [Test]
        public void IsUniqueBySpan_ForUniqueArgument_ReturnsTrue()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IInputGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var item = colllection.IsUnique(name.Span);
            Assert.IsTrue((bool)item);
        }

        [Test]
        public void IsUniqueByString_ForNonUniqueArgument_ReturnsFalse()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IInputGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var fieldPart2 = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart2.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart2.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart2.Object);

            var item = colllection.IsUnique("thename");
            Assert.IsFalse((bool)item);
        }

        [Test]
        public void IsUniqueBySpan_ForNonUniqueArgument_ReturnsFalse()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IFieldDocumentPart>();
            var colllection = new DocumentInputObjectFieldCollection(owner.Object);

            var graphField = new Mock<IInputGraphField>();
            graphField.Setup(x => x.TypeExpression).Returns(new AspNet.Schemas.GraphTypeExpression("String"));
            graphField.Setup(x => x.Name).Returns(name.ToString());

            var fieldPart = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart.Object);

            var fieldPart2 = new Mock<IInputObjectFieldDocumentPart>();
            fieldPart2.Setup(x => x.Field).Returns(graphField.Object);
            fieldPart2.Setup(x => x.Name).Returns(name.ToString());

            colllection.AddField(fieldPart2.Object);

            var item = colllection.IsUnique(name.Span);
            Assert.IsFalse((bool)item);
        }
    }
}