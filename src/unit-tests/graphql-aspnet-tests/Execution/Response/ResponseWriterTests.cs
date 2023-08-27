// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.Response
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Response;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.Response;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public partial class ResponseWriterTests
    {
        [Test]
        public async Task WriteDouble_DataIsRendered()
        {
            var writer = new DefaultQueryResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IQueryResponseItem>();
            data.Add("item1", new ResponseSingleValue(5.01D));

            var fieldSet = Substitute.For<IQueryResponseFieldSet>();
            fieldSet.Fields.Returns(data);

            var queryResult = Substitute.For<IQueryExecutionResult>();
            queryResult.Messages.Returns(new GraphMessageCollection());
            queryResult.Data.Returns(fieldSet);

            await writer.WriteAsync(stream, queryResult);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": 5.01
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }

        [Test]
        public async Task WriteFloat_DataIsRendered()
        {
            var writer = new DefaultQueryResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IQueryResponseItem>();
            data.Add("item1", new ResponseSingleValue(3.01f));

            var fieldSet = Substitute.For<IQueryResponseFieldSet>();
            fieldSet.Fields.Returns(data);

            var queryResult = Substitute.For<IQueryExecutionResult>();
            queryResult.Messages.Returns(new GraphMessageCollection());
            queryResult.Data.Returns(fieldSet);

            await writer.WriteAsync(stream, queryResult);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": 3.01
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }

        [Test]
        public async Task WriteDecimal_DataIsRendered()
        {
            var writer = new DefaultQueryResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IQueryResponseItem>();
            data.Add("item1", new ResponseSingleValue(6.021m));

            var fieldSet = Substitute.For<IQueryResponseFieldSet>();
            fieldSet.Fields.Returns(data);

            var queryResult = Substitute.For<IQueryExecutionResult>();
            queryResult.Messages.Returns(new GraphMessageCollection());
            queryResult.Data.Returns(fieldSet);

            await writer.WriteAsync(stream, queryResult);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": 6.021
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }

        [Test]
        public async Task WriteLong_DataIsRendered()
        {
            var writer = new DefaultQueryResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IQueryResponseItem>();
            data.Add("item1", new ResponseSingleValue(15L));

            var fieldSet = Substitute.For<IQueryResponseFieldSet>();
            fieldSet.Fields.Returns(data);

            var queryResult = Substitute.For<IQueryExecutionResult>();
            queryResult.Messages.Returns(new GraphMessageCollection());
            queryResult.Data.Returns(fieldSet);

            await writer.WriteAsync(stream, queryResult);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": 15
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }

        [Test]
        public async Task WriteULong_DataIsRendered()
        {
            var writer = new DefaultQueryResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IQueryResponseItem>();
            data.Add("item1", new ResponseSingleValue(40UL));

            var fieldSet = Substitute.For<IQueryResponseFieldSet>();
            fieldSet.Fields.Returns(data);

            var queryResult = Substitute.For<IQueryExecutionResult>();
            queryResult.Messages.Returns(new GraphMessageCollection());
            queryResult.Data.Returns(fieldSet);

            await writer.WriteAsync(stream, queryResult);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": 40
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }

        [Test]
        public async Task WriteInt_DataIsRendered()
        {
            var writer = new DefaultQueryResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IQueryResponseItem>();
            data.Add("item1", new ResponseSingleValue(1000));

            var fieldSet = Substitute.For<IQueryResponseFieldSet>();
            fieldSet.Fields.Returns(data);

            var queryResult = Substitute.For<IQueryExecutionResult>();
            queryResult.Messages.Returns(new GraphMessageCollection());
            queryResult.Data.Returns(fieldSet);

            await writer.WriteAsync(stream, queryResult);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": 1000
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }

        [Test]
        public async Task WriteUint_DataIsRendered()
        {
            var writer = new DefaultQueryResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IQueryResponseItem>();
            data.Add("item1", new ResponseSingleValue(50000000U));

            var fieldSet = Substitute.For<IQueryResponseFieldSet>();
            fieldSet.Fields.Returns(data);

            var queryResult = Substitute.For<IQueryExecutionResult>();
            queryResult.Messages.Returns(new GraphMessageCollection());
            queryResult.Data.Returns(fieldSet);

            await writer.WriteAsync(stream, queryResult);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": 50000000
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }

        [Test]
        public async Task WriteByte_DataIsRendered()
        {
            var writer = new DefaultQueryResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IQueryResponseItem>();
            data.Add("item1", new ResponseSingleValue((byte)5));

            var fieldSet = Substitute.For<IQueryResponseFieldSet>();
            fieldSet.Fields.Returns(data);

            var queryResult = Substitute.For<IQueryExecutionResult>();
            queryResult.Messages.Returns(new GraphMessageCollection());
            queryResult.Data.Returns(fieldSet);

            await writer.WriteAsync(stream, queryResult);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": 5
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }

        [Test]
        public async Task WriteSByte_DataIsRendered()
        {
            var writer = new DefaultQueryResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IQueryResponseItem>();
            data.Add("item1", new ResponseSingleValue((sbyte)-33));

            var fieldSet = Substitute.For<IQueryResponseFieldSet>();
            fieldSet.Fields.Returns(data);

            var queryResult = Substitute.For<IQueryExecutionResult>();
            queryResult.Messages.Returns(new GraphMessageCollection());
            queryResult.Data.Returns(fieldSet);

            await writer.WriteAsync(stream, queryResult);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": -33
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }

        [Test]
        public async Task WriteBool_DataIsRendered()
        {
            var writer = new DefaultQueryResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IQueryResponseItem>();
            data.Add("item1", new ResponseSingleValue(true));

            var fieldSet = Substitute.For<IQueryResponseFieldSet>();
            fieldSet.Fields.Returns(data);

            var queryResult = Substitute.For<IQueryExecutionResult>();
            queryResult.Messages.Returns(new GraphMessageCollection());
            queryResult.Data.Returns(fieldSet);

            await writer.WriteAsync(stream, queryResult);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": true
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }

        [Test]
        public async Task WriteDateTime_DataIsRendered()
        {
            var writer = new DefaultQueryResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IQueryResponseItem>();
            data.Add("item1", new ResponseSingleValue(new DateTime(2021, 10, 31, 5, 3, 1, DateTimeKind.Utc)));

            var fieldSet = Substitute.For<IQueryResponseFieldSet>();
            fieldSet.Fields.Returns(data);

            var queryResult = Substitute.For<IQueryExecutionResult>();
            queryResult.Messages.Returns(new GraphMessageCollection());
            queryResult.Data.Returns(fieldSet);

            await writer.WriteAsync(stream, queryResult);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": ""2021-10-31T05:03:01.000+00:00""
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }

        [Test]
        public async Task WriteDateTimeOffset_DataIsRendered()
        {
            var writer = new DefaultQueryResponseWriter<GraphSchema>(new GraphSchema());

            var stream = new MemoryStream();

            var data = new Dictionary<string, IQueryResponseItem>();
            data.Add("item1", new ResponseSingleValue(new DateTimeOffset(2021, 10, 31, 5, 3, 1, 0, TimeSpan.Zero)));

            var fieldSet = Substitute.For<IQueryResponseFieldSet>();
            fieldSet.Fields.Returns(data);

            var queryResult = Substitute.For<IQueryExecutionResult>();
            queryResult.Messages.Returns(new GraphMessageCollection());
            queryResult.Data.Returns(fieldSet);

            await writer.WriteAsync(stream, queryResult);

            stream.Seek(0, SeekOrigin.Begin);

            var actual = Encoding.UTF8.GetString(stream.ToArray());
            var expected = @"{
                ""data"": {
                    ""item1"": ""2021-10-31T05:03:01.000+00:00""
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expected, actual);
        }
    }
}