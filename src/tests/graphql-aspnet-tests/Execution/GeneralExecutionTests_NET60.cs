// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

#if NET6_0_OR_GREATER
namespace GraphQL.AspNet.Tests.Execution
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class GeneralExecutionTests_NET60
    {
        public class DateTimeController : GraphController
        {
            [QueryRoot]
            public DateOnly RetrieveDateOnly()
            {
                return new DateOnly(2021, 11, 13);
            }

            [QueryRoot]
            public TimeOnly RetrieveTimeOnly()
            {
                return new TimeOnly(09, 53, 00);
            }

            [QueryRoot]
            public bool ReceiveDateOnly(DateOnly date)
            {
                var testDate = new DateOnly(2021, 11, 13);
                return date == testDate;
            }

            [QueryRoot]
            public bool ReceiveTimeOnly(TimeOnly time)
            {
                var testTime = new TimeOnly(09, 53, 00);
                return time == testTime;
            }
        }

        [Test]
        public async Task DateOnly_AsObject_RenderedCorrectly()
        {
            var server = new TestServerBuilder()
                .AddGraphType<DateTimeController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("{ retrieveDateOnly }");

            var result = await server.RenderResult(builder);

            var expectedOutput = @"
            {
                ""data"": {
                    ""retrieveDateOnly"" : ""2021-11-13""
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task DateOnly_AsInputObject_FromString_RenderedCorrectly()
        {
            var server = new TestServerBuilder()
                .AddGraphType<DateTimeController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("{ receiveDateOnly(date: \"2021-11-13\") }");

            var result = await server.RenderResult(builder);

            var expectedOutput = @"
            {
                ""data"": {
                    ""receiveDateOnly"" : true
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task DateOnly_AsInputObject_FromNumber_RenderedCorrectly()
        {
            var server = new TestServerBuilder()
                .AddGraphType<DateTimeController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("{ receiveDateOnly(date: 1636822908) }");

            var result = await server.RenderResult(builder);

            var expectedOutput = @"
            {
                ""data"": {
                    ""receiveDateOnly"" : true
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task DateOnly_AsInputObject_FromStringVariable_RenderedCorrectly()
        {
            var server = new TestServerBuilder()
                .AddGraphType<DateTimeController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query getDate($var1: DateOnly!){ receiveDateOnly(date: $var1) }")
                .AddVariableData("{ \"var1\": \"2021-11-13\"}");

            var result = await server.RenderResult(builder);

            var expectedOutput = @"
            {
                ""data"": {
                    ""receiveDateOnly"" : true
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task DateOnly_AsInputObject_FromNumberVariable_RenderedCorrectly()
        {
            var server = new TestServerBuilder()
                .AddGraphType<DateTimeController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query getDate($var1: DateOnly!) { receiveDateOnly(date: $var1) }")
                .AddVariableData("{\"var1\" : 1636822908 }");

            var result = await server.RenderResult(builder);

            var expectedOutput = @"
            {
                ""data"": {
                    ""receiveDateOnly"" : true
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task TimeOnly_AsInputObject_FromString_RenderedCorrectly()
        {
            var server = new TestServerBuilder()
                .AddGraphType<DateTimeController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("{ receiveTimeOnly(time: \"09:53:00\") }");

            var result = await server.RenderResult(builder);

            var expectedOutput = @"
            {
                ""data"": {
                    ""receiveTimeOnly"" : true
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task TimeOnly_AsInputObject_FromStringVariable_RenderedCorrectly()
        {
            var server = new TestServerBuilder()
                .AddGraphType<DateTimeController>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query getTime($var1: TimeOnly!) { receiveTimeOnly(time: $var1) }")
                .AddVariableData("{\"var1\": \"09:53:00\" }");

            var result = await server.RenderResult(builder);

            var expectedOutput = @"
            {
                ""data"": {
                    ""receiveTimeOnly"" : true
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }
    }
}
#endif