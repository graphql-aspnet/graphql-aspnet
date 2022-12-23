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
    using System.Linq;
    using GraphQL.AspNet.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DocumentOperationCollectionTests
    {
        [Test]
        public void Add_AddsOperation()
        {
            var name = "thename";

            var owner = new Mock<IQueryDocument>();

            var operation = new Mock<IOperationDocumentPart>();
            operation.Setup(x => x.Name).Returns(name);

            var colllection = new DocumentOperationCollection(owner.Object);
            colllection.AddOperation(operation.Object);

            Assert.AreEqual(1, colllection.Count);
        }

        [Test]
        public void Keys_ContainsOperation()
        {
            var name = "thename";

            var owner = new Mock<IQueryDocument>();

            var operation = new Mock<IOperationDocumentPart>();
            operation.Setup(x => x.Name).Returns(name);

            var colllection = new DocumentOperationCollection(owner.Object);
            colllection.AddOperation(operation.Object);

            Assert.AreEqual(1, Enumerable.Count<string>(colllection.Keys));
            Assert.AreEqual("thename", Enumerable.First<string>(colllection.Keys));
        }

        [Test]
        public void ContainsKey_ExistingOperation_IsFound()
        {
            var name = "thename";

            var owner = new Mock<IQueryDocument>();

            var operation = new Mock<IOperationDocumentPart>();
            operation.Setup(x => x.Name).Returns(name);

            var colllection = new DocumentOperationCollection(owner.Object);
            colllection.AddOperation(operation.Object);

            var result = colllection.ContainsKey(name);
            Assert.IsTrue((bool)result);
        }

        [Test]
        public void ContainsKey_NotExistingOperation_IsFound()
        {
            var name = "thename";

            var owner = new Mock<IQueryDocument>();

            var operation = new Mock<IOperationDocumentPart>();
            operation.Setup(x => x.Name).Returns(name);

            var colllection = new DocumentOperationCollection(owner.Object);
            colllection.AddOperation(operation.Object);

            var result = colllection.ContainsKey("otherName");
            Assert.IsFalse((bool)result);
        }

        [Test]
        public void Add_WithNullName_AddsOperation()
        {
            string name = null;

            var owner = new Mock<IQueryDocument>();

            var operation = new Mock<IOperationDocumentPart>();
            operation.Setup(x => x.Name).Returns(name);

            var colllection = new DocumentOperationCollection(owner.Object);
            colllection.AddOperation(operation.Object);

            Assert.AreEqual(1, colllection.Count);
            Assert.IsTrue((bool)colllection.ContainsKey(string.Empty));
        }

        [Test]
        public void Add_NameIsTrimmed_AddsOperation()
        {
            string name = "long name     ";

            var owner = new Mock<IQueryDocument>();

            var operation = new Mock<IOperationDocumentPart>();
            operation.Setup(x => x.Name).Returns(name);

            var colllection = new DocumentOperationCollection(owner.Object);
            colllection.AddOperation(operation.Object);

            Assert.AreEqual(1, colllection.Count);
            Assert.IsTrue((bool)colllection.ContainsKey("long name"));
        }

        [Test]
        public void RetrieveOperation_ByName_FindsOperation()
        {
            string name = "thename";

            var owner = new Mock<IQueryDocument>();

            var operation = new Mock<IOperationDocumentPart>();
            operation.Setup(x => x.Name).Returns(name);

            var colllection = new DocumentOperationCollection(owner.Object);
            colllection.AddOperation(operation.Object);

            var found = colllection.RetrieveOperation(name);
            Assert.AreEqual(operation.Object, found);
        }

        [Test]
        public void RetrieveOperation_ByNotName_FindsNull()
        {
            string name = "thename";

            var owner = new Mock<IQueryDocument>();

            var operation = new Mock<IOperationDocumentPart>();
            operation.Setup(x => x.Name).Returns(name);

            var colllection = new DocumentOperationCollection(owner.Object);
            colllection.AddOperation(operation.Object);

            var found = colllection.RetrieveOperation("otehrName");
            Assert.IsNull(found);
        }

        [Test]
        public void RetrieveOperation_ByAnonymous_WithEmptyString_FindsOperation()
        {
            string name = string.Empty;

            var owner = new Mock<IQueryDocument>();

            var operation = new Mock<IOperationDocumentPart>();
            operation.Setup(x => x.Name).Returns(name);

            var colllection = new DocumentOperationCollection(owner.Object);
            colllection.AddOperation(operation.Object);

            var found = colllection.RetrieveOperation(string.Empty);
            Assert.AreEqual(operation.Object, found);
        }

        [Test]
        public void RetrieveOperation_ByAnonymous_WithNullString_FindsOperation()
        {
            string name = string.Empty;

            var owner = new Mock<IQueryDocument>();

            var operation = new Mock<IOperationDocumentPart>();
            operation.Setup(x => x.Name).Returns(name);

            var colllection = new DocumentOperationCollection(owner.Object);
            colllection.AddOperation(operation.Object);

            var found = colllection.RetrieveOperation(null);
            Assert.AreEqual(operation.Object, found);
        }

        [Test]
        public void RetrieveOperation_ByAnonymous_WithWhiteSpaceString_FindsOperation()
        {
            string name = string.Empty;

            var owner = new Mock<IQueryDocument>();

            var operation = new Mock<IOperationDocumentPart>();
            operation.Setup(x => x.Name).Returns(name);

            var colllection = new DocumentOperationCollection(owner.Object);
            colllection.AddOperation(operation.Object);

            var found = colllection.RetrieveOperation("      ");
            Assert.AreEqual(operation.Object, found);
        }
    }
}