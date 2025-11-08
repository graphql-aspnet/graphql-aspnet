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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class OneOfDirectiveTests
    {
        [OneOf]
        [GraphType(InputName = "MyInputUnion")]
        public class InputUnionWithOneOfDirective
        {
            public string Prop1 { get; set; }

            public int? Prop2 { get; set; }
        }

        public class TestController : GraphController
        {
            [QueryRoot(typeof(string))]
            public IGraphActionResult SubmitValue(InputUnionWithOneOfDirective input)
            {
                if (input.Prop1 is not null)
                    return this.Ok(input.Prop1);
                if (input.Prop2.HasValue)
                    return this.Ok(input.Prop2.Value.ToString());

                return this.Error("Query Succeeded, but shouldn't have. @oneOf was not validated correctly");
            }
        }

        [Test]
        public async Task InputUnion_WhenSupplyingProp1_ReturnsExpectedData()
        {
            var server = new TestServerBuilder()
                .AddType<TestController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        submitValue(input: {prop1: ""value1"" })
                    }");

            var expectedResponse = @"
            {
                ""data"" : {
                    ""submitValue"":  ""value1""
                }
            }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedResponse,
                result);
        }

        [Test]
        public async Task InputUnion_WhenSupplyingProp2_ReturnsExpectedData()
        {
            var server = new TestServerBuilder()
                .AddType<TestController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        submitValue(input: {prop2: 15 })
                    }");

            var expectedResponse = @"
            {
                ""data"" : {
                    ""submitValue"":  ""15""
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
        public async Task InputUnion_WhenSupplyingUncoeracbleLiteralValues_RejectsQuery(string inputDeclaration)
        {
            var server = new TestServerBuilder()
                .AddType<TestController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query {submitValue(input: " + inputDeclaration + ")}");

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
        public async Task InputUnion_WhenSupplyingUncoercableVariables_RejectsQuery(
            string inputDeclaration,
            string variableDeclaration,
            string variableValue)
        {
            var server = new TestServerBuilder()
                .AddType<TestController>()
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