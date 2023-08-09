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
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Logging;
    using GraphQL.AspNet.Interfaces.Security;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Configuration;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Engine.TypeMakers;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Interfaces;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Schema;
    using GraphQL.AspNet.ServerExtensions.MultipartRequests.Web;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.ServerExtensions.MutlipartRequests.TestData;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Primitives;
    using NSubstitute;
    using NSubstitute.ExceptionExtensions;
    using NUnit.Framework;

    [TestFixture]
    public class MultipartRequestGraphQLHttpProcessorTests
    {
        private DateTime _staticFailDate = new DateTime(202, 3, 4, 13, 14, 15, DateTimeKind.Utc);

        private (HttpContext, MultipartRequestGraphQLHttpProcessor<GraphSchema>) CreateTestObjects(
            (string FieldName, string FieldValue)[] fields = null,
            (string FieldName, string FileName, string ContentType, string FileContents)[] files = null,
            string httpMethod = "POST",
            string contentType = "multipart/form-data",
            IGraphQLRuntime<GraphSchema> runtime = null)
        {
            var httpContext = new DefaultHttpContext();

            var fileCollection = new FormFileCollection();
            var fieldCollection = new Dictionary<string, StringValues>();
            if (fields != null)
            {
                foreach (var kvp in fields)
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

            var builder = new TestServerBuilder();

            GraphQLProviders.ScalarProvider.RegisterCustomScalar(typeof(FileUploadScalarGraphType));
            builder.AddSingleton<IFileUploadScalarValueMaker, DefaultFileUploadScalarValueMaker>();
            builder.AddGraphQL(o =>
            {
                o.AddController<MultiPartFileController>();
                o.ResponseOptions.TimeStampLocalizer = (d) => _staticFailDate;
            });

            var server = builder.Build();

            var processor = new MultipartRequestGraphQLHttpProcessor<GraphSchema>(
                server.Schema,
                runtime ?? server.ServiceProvider.GetService<IGraphQLRuntime<GraphSchema>>(),
                server.ServiceProvider.GetService<IQueryResponseWriter<GraphSchema>>(),
                new MultiPartHttpFormPayloadParser<GraphSchema>(
                    server.ServiceProvider.GetService<IFileUploadScalarValueMaker>(),
                    new MultipartRequestConfiguration<GraphSchema>()),
                server.ServiceProvider.GetService<IGraphEventLogger>());

            var scope = server.ServiceProvider.CreateScope();
            httpContext.RequestServices = scope.ServiceProvider;

            return (httpContext, processor);
        }

        private string ExtractResponseString(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);

            var result = reader.ReadToEnd();
            return result;
        }

        [Test]
        public async Task NotAMultiPartRequest_ParsesAsNormal()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint(true);

            var (context, processor) = this.CreateTestObjects();

            var _options = new JsonSerializerOptions();
            _options.PropertyNameCaseInsensitive = true;
            _options.AllowTrailingCommas = true;
            _options.ReadCommentHandling = JsonCommentHandling.Skip;

            var data = @"{ ""query"" : ""query { simple { property1 property2 } }"" }";

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(data);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            context.Request.Form = null;
            context.Request.ContentType = "application/json";
            context.Request.Body = stream;
            context.Request.ContentLength = stream.Length;

            await processor.InvokeAsync(context);
            var result = this.ExtractResponseString(context);

            var expectedResult = @"
            {
                ""data"": {
                    ""simple"": {
                        ""property1"": ""sample"",
                        ""property2"": 33,
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
            Assert.AreEqual(200, context.Response.StatusCode);
        }

        [Test]
        public async Task NonBatchedQuery_NoFiles_ReturnsStandardResult()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint(true);

            var (context, processor) = this.CreateTestObjects(
                fields: new[]
                {
                    ("operations", @"{ ""query"" : ""query { simple { property1 property2 } }"" }"),
                });

            await processor.InvokeAsync(context);
            var result = this.ExtractResponseString(context);

            var expectedResult = @"
            {
                ""data"": {
                    ""simple"": {
                        ""property1"": ""sample"",
                        ""property2"": 33,
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
            Assert.AreEqual(200, context.Response.StatusCode);
        }

        private string PrepQuery(string queryText)
        {
            return queryText
                   .Replace("\r", "\\r")
                   .Replace("\n", "\\n");
        }

        [Test]
        public async Task NonBatchedQuery_SingleFile_ReturnsStandardResult()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint(true);

            var query1 = @"
                query($inboundFile: Upload!)
                {
                    fileUpload(singleFile: $inboundFile) {
                        property1
                        property2
                    }
                }";

            var (context, processor) = this.CreateTestObjects(
                fields: new[]
                {
                    ("operations", @"{
                            ""query"" : """ + this.PrepQuery(query1) + @""",
                            ""variables"" : { ""inboundFile"": null }
                        }"),
                    ("map", @"
                        {
                            ""file1MapKey"": [""variables"", ""inboundFile""]
                        }"),
                },
                files: new[]
                {
                    ("file1MapKey", "file1.txt", "text/plain", "file data"),
                });

            await processor.InvokeAsync(context);
            var result = this.ExtractResponseString(context);

            var expectedResult = @"
            {
                ""data"": {
                    ""fileUpload"": {
                        ""property1"": ""file1MapKey - file1.txt - file data"",
                        ""property2"": 9,
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
            Assert.AreEqual(200, context.Response.StatusCode);
        }

        [Test]
        public async Task NonBatchedQuery_SingleFile_InvalidMapField_Errors()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint(true);

            var query1 = @"
                query($inboundFile: Upload!)
                {
                    fileUpload(singleFile: $inboundFile) {
                        property1
                        property2
                    }
                }";

            var (context, processor) = this.CreateTestObjects(
                fields: new[]
                {
                    ("operations", @"{
                            ""query"" : """ + this.PrepQuery(query1) + @""",
                            ""variables"" : { ""inboundFile"": null }
                        }"),
                    ("map", @"
                        {
                            ""file1MapKey"": [
                        }"),
                },
                files: new[]
                {
                    ("file1MapKey", "file1.txt", "text/plain", "file data"),
                });

            await processor.InvokeAsync(context);
            var result = this.ExtractResponseString(context);

            Assert.AreEqual(400, context.Response.StatusCode);
        }

        [Test]
        public async Task InvalidOperationsJson_HandlesParsingException()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint(true);
            var (context, processor) = this.CreateTestObjects(
                fields: new[]
                {
                    ("operations", @"{
                            ""query"" : ,
                            ""variables"" : { ""inboundFile"": null }
                        }"),
                });

            await processor.InvokeAsync(context);
            Assert.AreEqual(400, context.Response.StatusCode);
        }

        [Test]
        public async Task NoQueriesOnBatch_HandlesParsingException()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint(true);
            var (context, processor) = this.CreateTestObjects(
                fields: new[]
                {
                    ("operations", @"[]"),
                });

            await processor.InvokeAsync(context);
            Assert.AreEqual(400, context.Response.StatusCode);
        }

        [Test]
        public async Task MultiPartForm_NoFiles_ReturnsStandardResult()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint(true);

            var query1 = @"
                query
                {
                    providedValue(value: 3){
                        property1
                        property2
                    }
                }";

            var query2 = @"
                query
                {
                    providedValue(value: 15){
                        property1
                        property2
                    }
                }";

            var (context, processor) = this.CreateTestObjects(
                fields: new[]
                {
                    ("operations", @"[
                        {
                            ""query"" : """ + this.PrepQuery(query1) + @"""
                        },
                        {
                            ""query"" : """ + this.PrepQuery(query2) + @"""
                        }]"),
                });

            await processor.InvokeAsync(context);
            var result = this.ExtractResponseString(context);

            var expectedResult = @"[
            {
                ""data"": {
                    ""providedValue"": {
                        ""property1"": ""sample"",
                        ""property2"": 3
                    }
                }
            },
            {
                ""data"": {
                    ""providedValue"": {
                        ""property1"": ""sample"",
                        ""property2"": 15
                    }
                }
            }]";

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
            Assert.AreEqual(200, context.Response.StatusCode);
        }

        [Test]
        public async Task RuntimeThrowsException_CustomResultIsGenerated()
        {
            using var restorePoint = new GraphQLGlobalRestorePoint(true);

            var runtime = Substitute.For<IGraphQLRuntime<GraphSchema>>();
            runtime.ExecuteRequestAsync(
                Arg.Any<IServiceProvider>(),
                Arg.Any<IQueryExecutionRequest>(),
                Arg.Any<IUserSecurityContext>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
                .ThrowsAsync(
                    new Exception("Explicit Failure"));
            runtime.CreateRequest(Arg.Any<GraphQueryData>())
                .Returns(Substitute.For<IQueryExecutionRequest>());

            var (context, processor) = this.CreateTestObjects(
                fields: new[]
                {
                    ("operations", @"{
                            ""query"" : ""query doesnt matter for this test"",
                        }"),
                },
                runtime: runtime);

            await processor.InvokeAsync(context);
            var result = this.ExtractResponseString(context);

            var expectedResult = @"
            {
              ""errors"": [
                {
                  ""message"": """ + MultipartRequestGraphQLHttpProcessor<GraphSchema>.ERROR_INTERNAL_SERVER_ISSUE + @""",
                  ""extensions"": {
                    ""code"": """ + Constants.ErrorCodes.GENERAL_ERROR + @""",
                    ""timestamp"": """ + _staticFailDate.ToRfc3339String() + @""",
                    ""severity"": ""CRITICAL""
                  }
                }
              ]
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
            Assert.AreEqual(200, context.Response.StatusCode);
        }
    }
}