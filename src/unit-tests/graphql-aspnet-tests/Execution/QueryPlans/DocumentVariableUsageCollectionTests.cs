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
    using GraphQL.AspNet.Execution.QueryPlans.DocumentParts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class DocumentVariableUsageCollectionTests
    {
        [Test]
        public void OwnerIsReturnedCorrectly()
        {
            var name = "thename";

            var owner = Substitute.For<IReferenceDocumentPart>();

            var varUsage = Substitute.For<IVariableUsageDocumentPart>();
            varUsage.VariableName.Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner);

            Assert.AreEqual(owner, colllection.Owner);
        }

        [Test]
        public void Add_AddsItemToCollection()
        {
            var name = "thename";

            var owner = Substitute.For<IReferenceDocumentPart>();

            var varUsage = Substitute.For<IVariableUsageDocumentPart>();
            varUsage.VariableName.Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner);
            colllection.Add(varUsage);

            Assert.AreEqual(1, colllection.Count);
        }

        [Test]
        public void FindReferences_ByVariableName_ReturnsReferences()
        {
            var name = "thename";

            var owner = Substitute.For<IReferenceDocumentPart>();

            var varUsage = Substitute.For<IVariableUsageDocumentPart>();
            varUsage.VariableName.Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner);
            colllection.Add(varUsage);

            var refs = colllection.FindReferences("thename");

            Assert.AreEqual(1, Enumerable.Count(refs));
        }

        [Test]
        public void FindReferences_ByVariableNameForNoRefs_ReturnsEmptySet()
        {
            var name = "thename";

            var owner = Substitute.For<IReferenceDocumentPart>();

            var varUsage = Substitute.For<IVariableUsageDocumentPart>();
            varUsage.VariableName.Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner);
            colllection.Add(varUsage);

            var refs = colllection.FindReferences("theOthername");

            Assert.AreEqual(0, Enumerable.Count(refs));
        }

        [Test]
        public void FindReferences_ByVariableNameForNoName_ReturnsEmptySet()
        {
            var name = "thename";

            var owner = Substitute.For<IReferenceDocumentPart>();

            var varUsage = Substitute.For<IVariableUsageDocumentPart>();
            varUsage.VariableName.Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner);
            colllection.Add(varUsage);

            var refs = colllection.FindReferences(null);

            Assert.AreEqual(0, Enumerable.Count(refs));
        }

        [Test]
        public void HasUsages_ByVariableName_ReturnsTrueWhenFound()
        {
            var name = "thename";

            var owner = Substitute.For<IReferenceDocumentPart>();

            var varUsage = Substitute.For<IVariableUsageDocumentPart>();
            varUsage.VariableName.Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner);
            colllection.Add(varUsage);

            var result = colllection.HasUsages("thename");
            Assert.IsTrue((bool)result);
        }

        [Test]
        public void HasUsages_ByVariableName_ReturnsFalseWhenNotFound()
        {
            var name = "thename";

            var owner = Substitute.For<IReferenceDocumentPart>();

            var varUsage = Substitute.For<IVariableUsageDocumentPart>();
            varUsage.VariableName.Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner);
            colllection.Add(varUsage);

            var result = colllection.HasUsages("theOthername");
            Assert.IsFalse((bool)result);
        }

        [Test]
        public void HasUsages_ByVariableName_ForNoName_ReturnsFalseWhenNotFound()
        {
            var name = "thename";

            var owner = Substitute.For<IReferenceDocumentPart>();

            var varUsage = Substitute.For<IVariableUsageDocumentPart>();
            varUsage.VariableName.Returns(name);

            var colllection = new DocumentVariableUsageCollection(owner);
            colllection.Add(varUsage);

            var result = colllection.HasUsages(null);
            Assert.IsFalse((bool)result);
        }
    }
}