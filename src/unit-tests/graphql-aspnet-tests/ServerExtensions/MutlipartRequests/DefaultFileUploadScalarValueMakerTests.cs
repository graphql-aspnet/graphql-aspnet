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
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultFileUploadScalarValueMakerTests
    {
        [Test]
        public async Task CreateFileScalar_FromByteArray_DeliversBytes()
        {
            var data = "bob smith";
            var bytes = Encoding.UTF8.GetBytes(data);

            var maker = new DefaultFileUploadScalarValueMaker();

            var file = await maker.CreateFileScalarAsync("testKey", bytes);
            var stream = await file.OpenFileAsync();
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();

            Assert.AreEqual("testKey", file.MapKey);
            Assert.AreEqual(data, text);
        }

        [Test]
        public async Task CreateFileScalar_FromByteArray_EmptyText_DeliversEmptyStream()
        {
            var data = string.Empty;
            var bytes = Encoding.UTF8.GetBytes(data);

            var maker = new DefaultFileUploadScalarValueMaker();

            var file = await maker.CreateFileScalarAsync("testKey", bytes);
            var stream = await file.OpenFileAsync();
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();

            Assert.AreEqual("testKey", file.MapKey);
            Assert.AreEqual(data, text);
        }

        [Test]
        public async Task CreateFileScalar_FromByteArray_NullText_EmptyStream()
        {
            var maker = new DefaultFileUploadScalarValueMaker();

            var file = await maker.CreateFileScalarAsync("testKey", null as byte[]);
            var stream = await file.OpenFileAsync();
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();

            Assert.AreEqual("testKey", file.MapKey);
            Assert.AreEqual(string.Empty, text);
        }

        [Test]
        public async Task CreateFileScalar_FromFormFile_DeliversData()
        {
            var data = "bob smith";

            var streamIn = new MemoryStream();
            using var writer = new StreamWriter(streamIn, leaveOpen: true);
            writer.Write(data);
            await writer.FlushAsync();
            writer.Close();
            streamIn.Seek(0, SeekOrigin.Begin);

            var fileIn = new Mock<IFormFile>();
            fileIn.Setup(x => x.OpenReadStream())
                .Returns(streamIn);
            fileIn.Setup(x => x.FileName).Returns("test file.txt");
            fileIn.Setup(x => x.ContentType).Returns("test content type");
            fileIn.Setup(x => x.Name).Returns("test map key");
            fileIn.Setup(x => x.Headers).Returns(null as IHeaderDictionary);

            var bytes = Encoding.UTF8.GetBytes(data);

            var maker = new DefaultFileUploadScalarValueMaker();

            var fileOut = await maker.CreateFileScalarAsync(fileIn.Object);
            var stream = await fileOut.OpenFileAsync();
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();

            Assert.AreEqual(data, text);
            Assert.AreEqual("test map key", fileOut.MapKey);
            Assert.AreEqual("test file.txt", fileOut.FileName);
            Assert.AreEqual("test content type", fileOut.ContentType);
            Assert.IsNull(fileOut.Headers);
        }

        [Test]
        public async Task CreateFileScalar_FromFormFile_WithHeaders_DeliversData()
        {
            var data = "bob smith";

            var streamIn = new MemoryStream();
            using var writer = new StreamWriter(streamIn, leaveOpen: true);
            writer.Write(data);
            await writer.FlushAsync();
            writer.Close();
            streamIn.Seek(0, SeekOrigin.Begin);

            var fileIn = new Mock<IFormFile>();
            fileIn.Setup(x => x.OpenReadStream())
                .Returns(streamIn);
            fileIn.Setup(x => x.FileName).Returns("test file.txt");
            fileIn.Setup(x => x.ContentType).Returns("test content type");
            fileIn.Setup(x => x.Name).Returns("test map key");
            fileIn.Setup(x => x.Headers).Returns(new HeaderDictionary()
            {
                { "header1", "value1" },
            });

            var bytes = Encoding.UTF8.GetBytes(data);

            var maker = new DefaultFileUploadScalarValueMaker();

            var fileOut = await maker.CreateFileScalarAsync(fileIn.Object);
            var stream = await fileOut.OpenFileAsync();
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();

            Assert.AreEqual(data, text);
            Assert.AreEqual("test content type", fileOut.ContentType);
            Assert.AreEqual(1, fileOut.Headers.Count);
            Assert.AreEqual("value1", fileOut.Headers["header1"]);
        }

        [Test]
        public async Task CreateFileScalar_FromFormFile_EmptyData_DeliversEmptyStream()
        {
            var data = string.Empty;

            var streamIn = new MemoryStream();
            using var writer = new StreamWriter(streamIn, leaveOpen: true);
            writer.Write(data);
            await writer.FlushAsync();
            writer.Close();
            streamIn.Seek(0, SeekOrigin.Begin);

            var fileIn = new Mock<IFormFile>();
            fileIn.Setup(x => x.OpenReadStream())
                .Returns(streamIn);
            fileIn.Setup(x => x.FileName).Returns("test file.txt");
            fileIn.Setup(x => x.ContentType).Returns("test content type");
            fileIn.Setup(x => x.Name).Returns("test map key");
            fileIn.Setup(x => x.Headers).Returns(null as IHeaderDictionary);

            var bytes = Encoding.UTF8.GetBytes(data);

            var maker = new DefaultFileUploadScalarValueMaker();

            var fileOut = await maker.CreateFileScalarAsync(fileIn.Object);
            var stream = await fileOut.OpenFileAsync();
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();

            Assert.AreEqual(data, text);
        }

        [Test]
        public async Task CreateFileScalar_FromFormFile_NullStreams_DeliversStreamUnaltered()
        {
            var fileIn = new Mock<IFormFile>();
            fileIn.Setup(x => x.OpenReadStream()).Returns(null as Stream);
            fileIn.Setup(x => x.FileName).Returns("test file.txt");
            fileIn.Setup(x => x.ContentType).Returns("test content type");
            fileIn.Setup(x => x.Name).Returns("test map key");
            fileIn.Setup(x => x.Headers).Returns(null as IHeaderDictionary);

            var maker = new DefaultFileUploadScalarValueMaker();

            var fileOut = await maker.CreateFileScalarAsync(fileIn.Object);
            var stream = await fileOut.OpenFileAsync();

            Assert.IsNull(stream);
        }

        [Test]
        public async Task CreateFileScalar_FromFormFile_NullFile_ReturnsNull()
        {
            var maker = new DefaultFileUploadScalarValueMaker();

            var fileOut = await maker.CreateFileScalarAsync(null as IFormFile);
            Assert.IsNull(fileOut);
        }
    }
}