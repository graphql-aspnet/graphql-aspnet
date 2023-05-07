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
    using System.IO;
    using System.Threading.Tasks;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FormFileStreamContainerTests
    {
        [Test]
        public async Task StreamOnFormFile_IsReturned()
        {
            var formFile = new Mock<IFormFile>();
            using var stream = new MemoryStream();

            formFile.Setup(x => x.OpenReadStream()).Returns(stream);

            var container = new FormFileStreamContainer(formFile.Object);

            var outputStream = await container.OpenFileStreamAsync();

            Assert.AreEqual(stream, outputStream);
        }
    }
}