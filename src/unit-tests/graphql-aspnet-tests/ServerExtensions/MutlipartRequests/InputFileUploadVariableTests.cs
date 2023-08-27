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
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Model;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class InputFileUploadVariableTests
    {
        [Test]
        public void PropertyCheck()
        {
            var file = new FileUpload("someKey", Substitute.For<IFileUploadStreamContainer>());

            var variable = new InputFileUploadVariable("variableKey", file);

            Assert.AreEqual("variableKey", variable.Name);
            Assert.AreEqual(file, variable.ResolvedValue);
            Assert.AreEqual(file, variable.Value);
        }

        [Test]
        public void NoFile_ExceptionThrown()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var variable = new InputFileUploadVariable("variableKey", value: null);
            });
        }

        [Test]
        public void NoMapKey_ExceptionThrown()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var variable = new InputFileUploadVariable("variableKey", mapKey: null);
            });
        }
    }
}