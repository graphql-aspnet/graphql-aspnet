// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.ServerExtensions.MutlipartRequests
{
    using System;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Schema;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class FileUploadScalarGraphTypeTests
    {
        [Test]
        public void Serialize_File_ReturnsName()
        {
            var file = new FileUpload(
                "key",
                Substitute.For<IFileUploadStreamContainer>(),
                fileName: "file.name");

            var scalar = new FileUploadScalarGraphType();

            var serialized = scalar.Serialize(file);
            Assert.AreEqual("file.name", serialized);
        }

        [Test]
        public void Serialize_NonFile_ReturnsNull()
        {
            var scalar = new FileUploadScalarGraphType();

            var serialized = scalar.Serialize(45);
            Assert.IsNull(serialized);
        }

        [Test]
        public void SerializeToQueryLanguage_Null_ReturnsNullString()
        {
            var scalar = new FileUploadScalarGraphType();
            var serialized = scalar.SerializeToQueryLanguage(null);
            Assert.AreEqual("null", serialized);
        }

        [Test]
        public void SerializeToQueryLanguage_NonNull_ThrowsException()
        {
            var scalar = new FileUploadScalarGraphType();

            Assert.Throws<NotSupportedException>(() =>
            {
                scalar.SerializeToQueryLanguage(45);
            });
        }

        [Test]
        public void ResolveSpan_ThrowsException()
        {
            var scalar = new FileUploadScalarGraphType();

            Assert.Throws<NotSupportedException>(() =>
            {
                var data = "data".AsSpan();
                scalar.Resolve(data);
            });
        }

        [Test]
        public void ScalarValueType_IsAnyType()
        {
            var scalar = new FileUploadScalarGraphType();
            Assert.IsTrue(scalar.ValueType.HasFlag(ScalarValueType.Number));
            Assert.IsTrue(scalar.ValueType.HasFlag(ScalarValueType.String));
            Assert.IsTrue(scalar.ValueType.HasFlag(ScalarValueType.Boolean));
        }

        [Test]
        public void Scalar_HasNoOtherKnownTypes()
        {
            var scalar = new FileUploadScalarGraphType();
            Assert.AreEqual(0, scalar.OtherKnownTypes.Count);
        }
    }
}