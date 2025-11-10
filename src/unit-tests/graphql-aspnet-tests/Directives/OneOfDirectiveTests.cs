// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Directives
{
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Tests.Directives.DirectiveTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class OneOfDirectiveTests
    {
        [TestCase("{prop1: \"value1\"}")]
        [TestCase("{prop2: 15}")]
        public async Task ArgumentAsInputUnion_ReturnsExpectedData_WhenValid(string argumentValue)
        {
            var server = new TestServerBuilder()
                .AddType<OneOfDirectiveController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query { submitSingleValue(input: " + argumentValue + ")}");

            var expectedResponse = @"
            {
                ""data"" : {
                    ""submitSingleValue"":  ""success""
                }
            }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedResponse,
                result);
        }

        [TestCase("[{prop1: \"value1\"}]")]
        [TestCase("[{prop2: 15}]")]
        [TestCase("[{prop2: 15},{prop1: \"value1\"}]")]
        [TestCase("[{prop2: 15},null, {prop1: \"value1\"}]")] // null should be successfully skipped
        [TestCase("[]")] // empty set is fine. the @oneOf directive applies to the items in the list
        public async Task ArgumentAsListOfInputUnion_ReturnsExpectedData_WhenValid(string argumentValue)
        {
            var server = new TestServerBuilder()
                .AddType<OneOfDirectiveController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query { submitListOfValues(inputs: " + argumentValue + ")}");

            var expectedResponse = @"
            {
                ""data"" : {
                    ""submitListOfValues"":  ""success""
                }
            }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedResponse,
                result);
        }

        [TestCase("{prop1: \"value\", prop2: 13}")] // cant supply both
        [TestCase("{prop1: \"value\", prop2: null}")] // cant supply both even if one is null
        [TestCase("{prop1: null, prop2: 13}")] // cant supply both even if one is null
        [TestCase("{prop1: null, prop2: null}")] // cant supply both as null
        [TestCase("{prop1: null}")] // cant supply just one if supplied as null
        [TestCase("{}")] // can't supply nothing
        public async Task ArgumentAsInputUnion_WhenSupplyingUncoeracbleLiteralValue_RejectsQuery(string inputDeclaration)
        {
            var server = new TestServerBuilder()
                .AddType<OneOfDirectiveController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query {submitSingleValue(input: " + inputDeclaration + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.False);
            Assert.That(result.Messages.Count, Is.EqualTo(1));

            // must only be one rule (the rule specific to @oneOf)
            var msg = result.Messages.Single();
            Assert.That(msg.Severity, Is.EqualTo(GraphMessageSeverity.Critical));
            Assert.That(msg.MetaData["Rule"].ToString(), Is.EqualTo("3.10.1"));
        }

        [TestCase("[{prop1: \"value\", prop2: 13}]")] // cant supply both
        [TestCase("[{prop1: \"value\", prop2: null}]")] // cant supply both even if one is null
        [TestCase("[{prop1: null, prop2: 13}]")] // cant supply both even if 'the other' is null
        [TestCase("[{prop1: null, prop2: null}]")] // cant supply both as null
        [TestCase("[{prop2: 15},{prop1: null, prop2: null}]")] // one of two items is wrong
        [TestCase("[{prop1: null}]")] // cant supply just one if supplied as null
        [TestCase("[{}]")] // can't supply nothing
        public async Task ArgumentAsListOfInputUnions_WhenSupplyingUncoeracbleLiteralValue_RejectsQuery(string inputDeclaration)
        {
            var server = new TestServerBuilder()
                .AddType<OneOfDirectiveController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query {submitListOfValues(inputs: " + inputDeclaration + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.False);
            Assert.That(result.Messages.Count, Is.EqualTo(1));

            // must only be one rule (the rule specific to @oneOf)
            var msg = result.Messages.Single();
            Assert.That(msg.Severity, Is.EqualTo(GraphMessageSeverity.Critical));
            Assert.That(msg.MetaData["Rule"].ToString(), Is.EqualTo("3.10.1"));
        }

        [TestCase("{prop1: $arg }", "String", "null")] // cant supply a field value that is null
        [TestCase("$arg", "MyInputUnion", "{\"prop1\": \"value\", \"prop2\": 13}")] // cant supply both props to a variable
        [TestCase("$arg", "MyInputUnion", "{\"prop1\": \"value\", \"prop2\": null}")] // cant supply both props even if one is null to a variable
        [TestCase("$arg", "MyInputUnion", "{\"prop1\": null, \"prop2\": 13}")] // cant supply both props even if one is null to a variable
        [TestCase("$arg", "MyInputUnion", "{\"prop1\": null, \"prop2\": null}")] // cant supply both props as null to a variable
        [TestCase("$arg", "MyInputUnion", "{\"prop1\": null}")] // cant supply just one if supplied as null to a variable
        [TestCase("$arg", "MyInputUnion", "{}")] // can't supply empty object to a variable
        public async Task ArgumentAsInputUnion_WhenSupplyingUncoercableVariableData_RejectsQuery(
            string inputDeclaration,
            string variableDeclaration,
            string variableValue)
        {
            var server = new TestServerBuilder()
                .AddType<OneOfDirectiveController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddVariableData("{ \"arg\": " + variableValue + "}")
                .AddQueryText("query ($arg: " + variableDeclaration + "){submitValue(input: " + inputDeclaration + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.False);
            Assert.That(result.Messages.Count, Is.EqualTo(1));

            // must only be one rule (the rule specific to @oneOf)
            var msg = result.Messages.Single();
            Assert.That(msg.Severity, Is.EqualTo(GraphMessageSeverity.Critical));
            Assert.That(msg.MetaData.ContainsKey("Rule"), Is.True);
            Assert.That(msg.MetaData["Rule"].ToString(), Is.EqualTo("3.10.1"));
        }
    }
}