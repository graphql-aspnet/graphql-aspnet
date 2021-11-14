// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

#if NET6_0_OR_GREATER
namespace GraphQL.AspNet.Tests.Response
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Response;
    using GraphQL.AspNet.Response;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public partial class ResponseWriterTests
    {
        [Test]
        public async Task WriteDateOnly_DataIsRendered()
        {
            var writer = new DefaultResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IResponseItem>();
            data.Add("item1", new ResponseSingleValue(new DateOnly(2021, 11, 11)));

            var fieldSet = new Mock<IResponseFieldSet>();
            fieldSet.Setup(x => x.Fields).Returns(data);

            var operationResult = new Mock<IGraphOperationResult>();
            operationResult.Setup(x => x.Messages).Returns(new GraphMessageCollection());
            operationResult.Setup(x => x.Data).Returns(fieldSet.Object);

            await writer.WriteAsync(stream, operationResult.Object);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": ""2021-11-11""
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }

        [Test]
        public async Task WriteTimeOnly_DataIsRendered()
        {
            var writer = new DefaultResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IResponseItem>();
            data.Add("item1", new ResponseSingleValue(new TimeOnly(8, 10, 9, 123)));

            var fieldSet = new Mock<IResponseFieldSet>();
            fieldSet.Setup(x => x.Fields).Returns(data);

            var operationResult = new Mock<IGraphOperationResult>();
            operationResult.Setup(x => x.Messages).Returns(new GraphMessageCollection());
            operationResult.Setup(x => x.Data).Returns(fieldSet.Object);

            await writer.WriteAsync(stream, operationResult.Object);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": ""08:10:09.123""
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }

        [Test]
        public async Task WriteTimeOnly_AllZeros_DataIsRendered()
        {
            var writer = new DefaultResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IResponseItem>();
            data.Add("item1", new ResponseSingleValue(new TimeOnly(0, 0, 0, 0)));

            var fieldSet = new Mock<IResponseFieldSet>();
            fieldSet.Setup(x => x.Fields).Returns(data);

            var operationResult = new Mock<IGraphOperationResult>();
            operationResult.Setup(x => x.Messages).Returns(new GraphMessageCollection());
            operationResult.Setup(x => x.Data).Returns(fieldSet.Object);

            await writer.WriteAsync(stream, operationResult.Object);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": ""00:00:00.000""
                }
            }";

            // ensure double digit hours min and second are rendered
            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }
    }
}
#endif