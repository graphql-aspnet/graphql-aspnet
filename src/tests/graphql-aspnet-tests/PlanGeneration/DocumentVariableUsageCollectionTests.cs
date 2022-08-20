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
    using System.Linq;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DocumentVariableUsageCollectionTests
    {
        [Test]
        public void OwnerIsReturnedCorrectly()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IReferenceDocumentPart>();

            var varUsage = new Mock<IVariableUsageDocumentPart>();
            varUsage.Setup(x => x.VariableName).Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner.Object);

            Assert.AreEqual(owner.Object, colllection.Owner);
        }

        [Test]
        public void Add_AddsItemToCollection()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IReferenceDocumentPart>();

            var varUsage = new Mock<IVariableUsageDocumentPart>();
            varUsage.Setup(x => x.VariableName).Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner.Object);
            colllection.Add(varUsage.Object);

            Assert.AreEqual(1, colllection.Count);
        }

        [Test]
        public void FindReferences_ByVariableName_ReturnsReferences()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IReferenceDocumentPart>();

            var varUsage = new Mock<IVariableUsageDocumentPart>();
            varUsage.Setup(x => x.VariableName).Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner.Object);
            colllection.Add(varUsage.Object);

            var refs = colllection.FindReferences("thename");

            Assert.AreEqual(1, refs.Count());
        }

        [Test]
        public void FindReferences_ByVariableNameForNoRefs_ReturnsEmptySet()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IReferenceDocumentPart>();

            var varUsage = new Mock<IVariableUsageDocumentPart>();
            varUsage.Setup(x => x.VariableName).Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner.Object);
            colllection.Add(varUsage.Object);

            var refs = colllection.FindReferences("theOthername");

            Assert.AreEqual(0, refs.Count());
        }

        [Test]
        public void FindReferences_ByVariableNameForNoName_ReturnsEmptySet()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IReferenceDocumentPart>();

            var varUsage = new Mock<IVariableUsageDocumentPart>();
            varUsage.Setup(x => x.VariableName).Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner.Object);
            colllection.Add(varUsage.Object);

            var refs = colllection.FindReferences(null);

            Assert.AreEqual(0, refs.Count());
        }

        [Test]
        public void HasUsages_ByVariableName_ReturnsTrueWhenFound()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IReferenceDocumentPart>();

            var varUsage = new Mock<IVariableUsageDocumentPart>();
            varUsage.Setup(x => x.VariableName).Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner.Object);
            colllection.Add(varUsage.Object);

            var result = colllection.HasUsages("thename");
            Assert.IsTrue(result);
        }

        [Test]
        public void HasUsages_ByVariableName_ReturnsFalseWhenNotFound()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IReferenceDocumentPart>();

            var varUsage = new Mock<IVariableUsageDocumentPart>();
            varUsage.Setup(x => x.VariableName).Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner.Object);
            colllection.Add(varUsage.Object);

            var result = colllection.HasUsages("theOthername");
            Assert.IsFalse(result);
        }

        [Test]
        public void HasUsages_ByVariableName_ForNoName_ReturnsFalseWhenNotFound()
        {
            var name = "thename".AsMemory();

            var owner = new Mock<IReferenceDocumentPart>();

            var varUsage = new Mock<IVariableUsageDocumentPart>();
            varUsage.Setup(x => x.VariableName).Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner.Object);
            colllection.Add(varUsage.Object);

            var result = colllection.HasUsages(null);
            Assert.IsFalse(result);
        }
    }
}