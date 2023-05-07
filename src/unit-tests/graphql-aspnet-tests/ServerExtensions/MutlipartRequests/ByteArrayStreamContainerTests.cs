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
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using NUnit.Framework;

    [TestFixture]
    public class ByteArrayStreamContainerTests
    {
        [TestCase("bob", "bob")]
        [TestCase(null, "")]
        [TestCase("", "")]
        public async Task ArrayToStreamTests(string testData, string expectedOutput)
        {
            byte[] array = testData != null
                ? Encoding.UTF8.GetBytes(testData)
                : null;

            var container = new ByteArrayStreamContainer(array);

            using var streamOut = await container.OpenFileStreamAsync();

            using var reader = new StreamReader(streamOut);
            var dataOut = reader.ReadToEnd();

            Assert.AreEqual(expectedOutput, dataOut);
        }
    }
}