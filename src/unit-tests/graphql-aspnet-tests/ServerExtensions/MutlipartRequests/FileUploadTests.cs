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
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using Microsoft.Extensions.Primitives;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FileUploadTests
    {
        [Test]
        public void PropertyCheck()
        {
            var headers = new Dictionary<string, StringValues>();
            var container = new Mock<IFileUploadStreamContainer>().Object;
            var file = new FileUpload(
                "testKey",
                container,
                "some content",
                "some file name",
                headers);

            Assert.AreEqual("testKey", file.MapKey);
            Assert.AreEqual("some content", file.ContentType);
            Assert.AreEqual("some file name", file.FileName);
            Assert.AreEqual(headers, file.Headers);
        }

        [Test]
        public void MapKey_IsRequired()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var file = new FileUpload(null, new Mock<IFileUploadStreamContainer>().Object);
            });
        }

        [Test]
        public void StreamContainer_IsRequired()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var file = new FileUpload("testKey", null);
            });
        }

        [Test]
        public void HeadersIsSetToNull_WhenOmitted()
        {
            var file = new FileUpload(
                "testKey",
                new Mock<IFileUploadStreamContainer>().Object,
                headers: null);

            Assert.IsNull(file.Headers);
        }

        [Test]
        public async Task OpenStream_ReturnsUnderlyingStream()
        {
            var memStream = new MemoryStream();

            var container = new Mock<IFileUploadStreamContainer>();
            container.Setup(x => x.OpenFileStreamAsync())
                .ReturnsAsync(memStream);

            var file = new FileUpload(
                "testKey",
                container.Object);

            var stream = await file.OpenFileAsync();

            Assert.AreEqual(memStream, stream);
        }
    }
}