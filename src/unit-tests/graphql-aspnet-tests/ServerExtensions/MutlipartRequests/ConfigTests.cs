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
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Configuration;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine.TypeMakers;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Exceptions;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.Tests.Execution.TestData.DirectiveProcessorTypeSystemLocationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Web.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Internal;
    using Microsoft.Extensions.Primitives;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigTests
    {
        private (MultiPartHttpFormPayloadParser<GraphSchema>, HttpContext) CreateTestObject(
             string operationsField,
             string mapField,
             IMultipartRequestConfiguration<GraphSchema> config = null,
             (string FieldName, string FieldValue)[] additionalFields = null,
             (string FieldName, string FileName, string ContentType, string FileContents)[] files = null,
             string httpMethod = "POST",
             string contentType = "multipart/form-data")
        {
            var httpContext = new DefaultHttpContext();

            var fileCollection = new FormFileCollection();
            var fieldCollection = new Dictionary<string, StringValues>();

            if (operationsField != null)
                fieldCollection.Add(MultipartRequestConstants.Web.OPERATIONS_FORM_KEY, operationsField);

            if (mapField != null)
                fieldCollection.Add(MultipartRequestConstants.Web.MAP_FORM_KEY, mapField);

            if (additionalFields != null)
            {
                foreach (var kvp in additionalFields)
                    fieldCollection.Add(kvp.FieldName, kvp.FieldValue);
            }

            if (files != null)
            {
                foreach (var item in files)
                {
                    byte[] fileBytes = null;
                    if (item.FileContents != null)
                        fileBytes = Encoding.UTF8.GetBytes(item.FileContents);

                    var formFile = new FormFile(
                            fileBytes != null
                                ? new MemoryStream(fileBytes)
                                : Stream.Null,
                            0,
                            fileBytes?.Length ?? 0,
                            item.FieldName,
                            item.FileName);

                    formFile.Headers = new HeaderDictionary();
                    if (item.ContentType != null)
                        formFile.Headers.Add("Content-Type", item.ContentType);

                    fileCollection.Add(formFile);
                }
            }

            var form = new FormCollection(fieldCollection, fileCollection);
            httpContext.Request.ContentType = contentType;
            httpContext.Request.Method = httpMethod;
            httpContext.Request.Form = form;
            httpContext.Response.Body = new MemoryStream();

            return (
                    new MultiPartHttpFormPayloadParser<GraphSchema>(
                        new DefaultFileUploadScalarValueMaker(),
                        config),
                    httpContext);
        }

        [TestCase(MultipartRequestMapHandlingMode.Default, "\"variables.var1\"",  false)]
        [TestCase(MultipartRequestMapHandlingMode.None, "\"variables.var1\"", true)]
        [TestCase(MultipartRequestMapHandlingMode.AllowStringPaths, "\"variables.var1\"", false)]
        [TestCase(MultipartRequestMapHandlingMode.None, @"[ ""variables"", ""var1"" ]", false)]
        public async Task StringMapValue(MultipartRequestMapHandlingMode mode, string mapValue, bool shouldThrow)
        {
            var operations = @"
                {
                    ""query""     : ""query { field1 {field2 field3} }"",
                    ""variables"" : { ""var1"": null, ""var2"": 3 },
                    ""operationName"" : ""bob""
                }";

            var config = new Mock<IMultipartRequestConfiguration<GraphSchema>>();
            config.Setup(x => x.RequestMode).Returns(MultipartRequestMode.Default);
            config.Setup(x => x.MapMode).Returns(mode);

            var map = @"{ ""0"": " + mapValue + "}";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var (parser, context) = this.CreateTestObject(
                operations,
                map,
                config.Object,
                files: new[] { file });

            if (shouldThrow)
            {
                var ex = Assert.ThrowsAsync<HttpContextParsingException>(async () =>
                {
                    await parser.ParseAsync(context);
                });

                Assert.IsTrue(ex.Message.Contains("This schema does not allow string based map values"));
            }
            else
            {
                var payload = await parser.ParseAsync(context);
                Assert.IsNotNull(payload);
            }
        }

        [TestCase(MultipartRequestMapHandlingMode.Default, @"[ ""variables.var1"" ]", false)]
        [TestCase(MultipartRequestMapHandlingMode.None, @"[ ""variables.var1"" ]", true)]
        [TestCase(MultipartRequestMapHandlingMode.SplitDotDelimitedSingleElementArrays, @"[ ""variables.var1"" ]", false)]
        public async Task SplitDotDelimitedSingleElementArrays(MultipartRequestMapHandlingMode mode, string mapValue, bool shouldThrow)
        {
            var operations = @"
                {
                    ""query""     : ""query { field1 {field2 field3} }"",
                    ""variables"" : { ""var1"": null, ""var2"": 3 },
                    ""variables.var1"" : ""an already set value"",
                    ""operationName"" : ""bob""
                }";

            var config = new Mock<IMultipartRequestConfiguration<GraphSchema>>();
            config.Setup(x => x.RequestMode).Returns(MultipartRequestMode.Default);
            config.Setup(x => x.MapMode).Returns(mode);

            var map = @"{ ""0"": " + mapValue + "}";

            var file = ("0", "myFile.txt", "text/plain", "testData");

            var (parser, context) = this.CreateTestObject(
                operations,
                map,
                config.Object,
                files: new[] { file });

            if (shouldThrow)
            {
                var ex = Assert.ThrowsAsync<InvalidMultiPartMapException>(async () =>
                {
                    await parser.ParseAsync(context);
                });

                Assert.AreEqual(typeof(JsonNodeException), ex.InnerException.GetType());
                Assert.IsTrue(ex.InnerException.Message.Contains("The location indicated by the query path already contains a value,"));
            }
            else
            {
                var payload = await parser.ParseAsync(context);
                Assert.IsNotNull(payload);
            }
        }

        [TestCase(1, true)]
        [TestCase(2, false)]
        [TestCase(3, false)]
        public async Task MaxFiles(int? maxAllowedFiles, bool shouldThrow)
        {
            var operations = @"
                {
                    ""query""     : ""query { field1 {field2 field3} }"",
                    ""variables"" : { ""var1"": null, ""var2"": null },
                    ""variables.var1"" : ""an already set value"",
                    ""operationName"" : ""bob""
                }";

            var config = new Mock<IMultipartRequestConfiguration<GraphSchema>>();
            config.Setup(x => x.RequestMode).Returns(MultipartRequestMode.Default);
            config.Setup(x => x.MapMode).Returns(MultipartRequestMapHandlingMode.Default);
            config.Setup(x => x.MaxFileCount).Returns(maxAllowedFiles);
            config.Setup(x => x.MaxBlobCount).Returns(1000);

            var map = @"{
                ""0"": ""variables.var1"",
                ""1"" : ""variables.var2""
            }";

            var file = ("0", "myFile.txt", "text/plain", "testData");
            var file1 = ("1", "myFile.txt", "text/plain", "testData");

            var (parser, context) = this.CreateTestObject(
                operations,
                map,
                config.Object,
                files: new[] { file, file1 });

            if (shouldThrow)
            {
                var ex = Assert.ThrowsAsync<HttpContextParsingException>(async () =>
                {
                    await parser.ParseAsync(context);
                });

                Assert.IsTrue(ex.Message.Contains("Maximum allowed files"));
            }
            else
            {
                var payload = await parser.ParseAsync(context);
                Assert.IsNotNull(payload);
            }
        }

        [TestCase(1, true)]
        [TestCase(2, false)]
        [TestCase(3, false)]
        public async Task MaxBlobs(int? maxAllowedBlobs, bool shouldThrow)
        {
            var operations = @"
                {
                    ""query""     : ""query { field1 {field2 field3} }"",
                    ""variables"" : { ""var1"": null, ""var2"": null },
                    ""variables.var1"" : ""an already set value"",
                    ""operationName"" : ""bob""
                }";

            var config = new Mock<IMultipartRequestConfiguration<GraphSchema>>();
            config.Setup(x => x.RequestMode).Returns(MultipartRequestMode.Default);
            config.Setup(x => x.MapMode).Returns(MultipartRequestMapHandlingMode.Default);
            config.Setup(x => x.MaxFileCount).Returns(1900);
            config.Setup(x => x.MaxBlobCount).Returns(maxAllowedBlobs);

            var map = @"{
                ""0"": ""variables.var1"",
                ""1"" : ""variables.var2""
            }";

            var blob = ("0", "testData");
            var blob1 = ("1",  "testData");

            var (parser, context) = this.CreateTestObject(
                operations,
                map,
                config.Object,
                additionalFields: new[] { blob, blob1 });

            if (shouldThrow)
            {
                var ex = Assert.ThrowsAsync<HttpContextParsingException>(async () =>
                {
                    await parser.ParseAsync(context);
                });

                Assert.IsTrue(ex.Message.Contains("Maximum allowed files"));
            }
            else
            {
                var payload = await parser.ParseAsync(context);
                Assert.IsNotNull(payload);
            }
        }
    }
}