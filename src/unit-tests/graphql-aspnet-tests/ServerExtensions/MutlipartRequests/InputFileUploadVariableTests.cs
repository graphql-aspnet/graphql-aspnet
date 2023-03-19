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
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class InputFileUploadVariableTests
    {
        [Test]
        public void PropertyCheck()
        {
            var file = new FileUpload("someKey", new Mock<IFileUploadStreamContainer>().Object);

            var variable = new InputFileUploadVariable("variableKey", file);

            Assert.AreEqual("variableKey", variable.Name);
            Assert.AreEqual(file, variable.ResolvedValue);
            Assert.AreEqual(file, variable.Value);
        }

        [Test]
        public void NoFile_NullIsResolved()
        {
            var variable = new InputFileUploadVariable("variableKey", null);

            Assert.AreEqual("variableKey", variable.Name);
            Assert.IsNull(variable.ResolvedValue);
            Assert.IsNull(variable.Value);
        }
    }
}