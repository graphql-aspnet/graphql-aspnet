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
        [TestCase("null")]
        public async Task ArgumentAsInputUnion_ObjectLiteral_WhenSupplyingCoercableData_IsSuccessful(string argumentValue)
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

        [TestCase("{prop1: \"value\", prop2: 13}")] // cant supply both
        [TestCase("{prop1: \"value\", prop2: null}")] // cant supply both even if one is null
        [TestCase("{prop1: null, prop2: 13}")] // cant supply both even if one is null
        [TestCase("{prop1: null, prop2: null}")] // cant supply both as null
        [TestCase("{prop1: null}")] // cant supply just one if supplied as null
        [TestCase("{}")] // can't supply nothing
        public async Task ArgumentAsInputUnion_ObjectLiteral_WhenSupplyingUncoeracbleData_RejectsQuery(string inputDeclaration)
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

        [TestCase("{prop1: $arg }", "String", "\"someValue\"")] // if the value ont he prop of a union is not null, we're good
        [TestCase("{prop2: $arg }", "Int!", "15")] // if the value ont he prop of a union is not null, we're good
        [TestCase("$arg", "MyInputUnion", "{\"prop2\": 15}")] // valid object with a single prop
        [TestCase("$arg", "MyInputUnion", "{\"prop1\": \"value1\"}")] // valid object with a single prop (other prop)
        [TestCase("$arg", "MyInputUnion", "null")] // we can supply straight up null, since the type expression allows it
        public async Task ArgumentAsInputUnion_VariableData_WhenSupplyingCoercableData_IsSuccessful(
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
                .AddQueryText("query ($arg: " + variableDeclaration + "){submitSingleValue(input: " + inputDeclaration + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.True);
            Assert.That(result.Messages.Count, Is.EqualTo(0));
        }

        [TestCase("{prop1: $arg }", "String", "null")] // cant supply a field value that is null
        [TestCase("$arg", "MyInputUnion", "{\"prop1\": \"value\", \"prop2\": 13}")] // cant supply both props to a variable
        [TestCase("$arg", "MyInputUnion", "{\"prop1\": \"value\", \"prop2\": null}")] // cant supply both props even if one is null to a variable
        [TestCase("$arg", "MyInputUnion", "{\"prop1\": null, \"prop2\": 13}")] // cant supply both props even if one is null to a variable
        [TestCase("$arg", "MyInputUnion", "{\"prop1\": null, \"prop2\": null}")] // cant supply both props as null to a variable
        [TestCase("$arg", "MyInputUnion", "{\"notAProp1\": 13, \"notAProp2\": 15}")] // cant supply just extra fields
        [TestCase("$arg", "MyInputUnion", "{\"prop1\": \"value1\", \"notAProp\": \"value\"}")] // cant supply extra fields with valid fields
        [TestCase("$arg", "MyInputUnion", "{\"prop1\": \"value1\", \"notAProp\": null}")] // cant supply extra fields even if null
        [TestCase("$arg", "MyInputUnion", "{\"prop1\": null}")] // cant supply just one if supplied as null to a variable
        [TestCase("$arg", "MyInputUnion", "{}")] // can't supply empty object to a variable
        public async Task ArgumentAsInputUnion_VariableData_WhenSupplyingUncoercableData_RejectsQuery(
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
                .AddQueryText("query ($arg: " + variableDeclaration + "){submitSingleValue(input: " + inputDeclaration + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.False);
            Assert.That(result.Messages.Count, Is.EqualTo(1));

            // must only be one rule (the rule specific to @oneOf)
            var msg = result.Messages.Single();
            Assert.That(msg.Severity, Is.EqualTo(GraphMessageSeverity.Critical));
            Assert.That(msg.MetaData.ContainsKey("Rule"), Is.True);
            Assert.That(msg.MetaData["Rule"].ToString(), Is.EqualTo("3.10.1"));
        }

        [TestCase("[{prop1: \"value1\"}]")]
        [TestCase("[{prop2: 15}]")]
        [TestCase("[{prop2: 15},{prop1: \"value1\"}]")]
        [TestCase("[{prop2: 15},null, {prop1: \"value1\"}]")] // null should be successfully skipped
        [TestCase("[]")] // empty set is fine. the @oneOf directive applies to the items in the list
        public async Task ArgumentAsListOfInputUnion_ObjectLiteral_WhenSupplyingCoeracbleData_IsSuccessful(string argumentValue)
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

        [TestCase("[{prop1: \"value\", prop2: 13}]")] // cant supply both
        [TestCase("[{prop1: \"value\", prop2: null}]")] // cant supply both even if one is null
        [TestCase("[{prop1: null, prop2: 13}]")] // cant supply both even if 'the other' is null
        [TestCase("[{prop1: null, prop2: null}]")] // cant supply both as null
        [TestCase("[{prop2: 15},{prop1: null, prop2: null}]")] // one of two items is wrong
        [TestCase("[{prop1: null}]")] // cant supply just one if supplied as null
        [TestCase("[{}]")] // can't supply nothing
        public async Task ArgumentAsListOfInputUnions_ObjectLiteral_WhenSupplyingUncoeracbleData_RejectsQuery(string inputDeclaration)
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

        [TestCase("[{prop1: $arg }]", "String", "\"someValue\"")] // if the value ont he prop of a union in a list is not null, we're good
        [TestCase("[{prop2: $arg }]", "Int!", "15")] // if the value ont he prop of a union is not null, we're good
        [TestCase("$arg", "[MyInputUnion]", "[{\"prop2\": 15}]")] // list with a valid object with a single prop
        [TestCase("$arg", "[MyInputUnion]", "[{\"prop2\": 15}, null, {\"prop1\": \"strvalue\"}]")] // a list with multiple valid objects and a null
        [TestCase("$arg", "[MyInputUnion]", "[]")] // an empty list is valid, the arg validates the list not the unions in the list
        [TestCase("$arg", "[MyInputUnion]", "null")] // we can supply straight up null, since the type expression allows it in this case
        public async Task ArgumentAsListOfInputUnion_VariableData_WhenSupplyingCoercableData_IsSuccessful(
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
                .AddQueryText("query ($arg: " + variableDeclaration + "){submitListOfValues(inputs: " + inputDeclaration + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.True);
            Assert.That(result.Messages.Count, Is.EqualTo(0));
        }

        [TestCase("[{prop1: $arg }]", "String", "null")] // if the value ont he prop of a union in a list is null
        [TestCase("[{prop2: $arg }]", "Int", "null")] // if the value ont he prop of a union in a list is null
        [TestCase("$arg", "[MyInputUnion]", "[{\"prop2\": null}]")] // list with an object that has an a nulled field
        [TestCase("$arg", "[MyInputUnion]", "[{\"prop2\": 15, \"prop1\": \"someVal\"}]")] // list with an object that has more than one field
        [TestCase("$arg", "[MyInputUnion]", "[{\"prop2\": 15, \"prop1\": null}]")] // list with an object that has more than one field and one is null
        [TestCase("$arg", "[MyInputUnion]", "[{}]")] // list with an object that has no fields
        [TestCase("$arg", "[MyInputUnion]", "[{\"prop2\": 15}, null, {\"prop1\": null}]")] // a list with multiple valid objects and an invalid
        public async Task ArgumentAsListOfInputUnion_VariableData_WhenSupplyingUncoercableData_RejectsQuery(
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
                .AddQueryText("query ($arg: " + variableDeclaration + "){submitListOfValues(inputs: " + inputDeclaration + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.False);
            Assert.That(result.Messages.Count, Is.EqualTo(1));

            // must only be one rule (the rule specific to @oneOf)
            var msg = result.Messages.Single();
            Assert.That(msg.Severity, Is.EqualTo(GraphMessageSeverity.Critical));
            Assert.That(msg.MetaData.ContainsKey("Rule"), Is.True);
            Assert.That(msg.MetaData["Rule"].ToString(), Is.EqualTo("3.10.1"));
        }

        [TestCase("{prop0: \"value1\", child: {prop1: \"value1\"}}")]
        [TestCase("{prop0: \"value1\", child: {prop2: 15}}")]
        [TestCase("{prop0: \"value1\", child: null}")]
        public async Task ArgumentWithChildInputUnion_ObjectLiteral_WhenSupplyingCoercableData_IsSuccessful(string argumentValue)
        {
            var server = new TestServerBuilder()
                .AddType<OneOfDirectiveController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query { submitSingleValueWithChildUnion(input: " + argumentValue + ")}");

            var expectedResponse = @"
            {
                ""data"" : {
                    ""submitSingleValueWithChildUnion"":  ""success""
                }
            }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedResponse,
                result);
        }

        [TestCase("{prop0: \"value\", child: {prop1: \"value\", prop2: 13}}")] // cant supply both
        [TestCase("{prop0: \"value\", child: {prop1: \"value\", prop2: null}}")] // cant supply both even if one is null
        [TestCase("{prop0: \"value\", child: {prop1: null, prop2: 13}}")] // cant supply both even if one is null
        [TestCase("{prop0: \"value\", child: {prop1: null, prop2: null}}")] // cant supply both as null
        [TestCase("{prop0: \"value\", child: {prop1: null}}")] // cant supply just one if supplied as null
        [TestCase("{prop0: \"value\", child: {prop2: null}}")] // cant supply just one if supplied as null
        [TestCase("{prop0: \"value\", child: {}}")] // can't supply nothing
        public async Task ArgumentWithChildInputUnion_ObjectLiteral_WhenSupplyingUncoeracbleData_RejectsQuery(string inputDeclaration)
        {
            var server = new TestServerBuilder()
                .AddType<OneOfDirectiveController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query {submitSingleValueWithChildUnion(input: " + inputDeclaration + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.False);
            Assert.That(result.Messages.Count, Is.EqualTo(1));

            // must only be one rule (the rule specific to @oneOf)
            var msg = result.Messages.Single();
            Assert.That(msg.Severity, Is.EqualTo(GraphMessageSeverity.Critical));
            Assert.That(msg.MetaData["Rule"].ToString(), Is.EqualTo("3.10.1"));
        }

        [TestCase("{prop0: \"val1\", child: {prop1: $arg }}", "String", "\"someValue\"")] // if the value ont he prop of a union is not null, we're good
        [TestCase("{prop0: \"val1\", child: {prop2: $arg }}", "Int!", "15")] // if the value ont he prop of a union is not null, we're good
        [TestCase("{prop0: \"val1\", child: $arg}", "MyInputUnion", "{\"prop2\": 15}")] // valid object with a single prop
        [TestCase("{prop0: \"val1\", child: $arg}", "MyInputUnion", "{\"prop1\": \"value1\"}")] // valid object with a single prop (other prop)
        [TestCase("{prop0: \"val1\", child: $arg}", "MyInputUnion", "null")] // we can supply straight up null, since the type expression allows it
        [TestCase("$arg", "ItemWithChildUnion", "null")] // we can supply straight up null, since the type expression allows it, child union should be null as well which is fine
        public async Task ArgumentWithChildInputUnion_VariableData_WhenSupplyingCoercableData_IsSuccessful(
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
                .AddQueryText("query ($arg: " + variableDeclaration + "){submitSingleValueWithChildUnion(input: " + inputDeclaration + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.True);
            Assert.That(result.Messages.Count, Is.EqualTo(0));
        }

        // variable supplied as an input union to an object literal
        [TestCase("{prop0: \"val1\", child: {prop1: $arg }}", "String", "null")] // value cant be null
        [TestCase("{prop0: \"val1\", child: {prop2: $arg }}", "Int", "null")] // value cant be null
        [TestCase("{prop0: \"val1\", child: $arg}", "MyInputUnion", "{\"prop1\": \"value1\", \"prop2\": 15}")] // cant supply both
        [TestCase("{prop0: \"val1\", child: $arg}", "MyInputUnion", "{\"prop1\": \"value1\", \"prop2\": null}")] // cant supply both even if one is null
        [TestCase("{prop0: \"val1\", child: $arg}", "MyInputUnion", "{\"prop1\": null, \"prop2\": 15}")] // cant supply both even if other is null
        [TestCase("{prop0: \"val1\", child: $arg}", "MyInputUnion", "{\"prop1\": \"value\", \"notAProp\": 15}")] // cant supply invalid props
        [TestCase("{prop0: \"val1\", child: $arg}", "MyInputUnion", "{\"prop1\": \"value\", \"notAProp\": null}")] // cant supply invalid props even when null
        [TestCase("{prop0: \"val1\", child: $arg}", "MyInputUnion", "{\"notAProp\": \"value\", \"notAProp2\" : 15}")] // cant supply just invalid props
        [TestCase("{prop0: \"val1\", child: $arg}", "MyInputUnion", "{}")] // cant supply no fields

        // variable supplied as an input argument that embeds a would be input union as part of itself
        [TestCase("$arg", "ItemWithChildUnion", "{\"prop0\": \"val1\", \"child\":{\"prop1\": \"value1\", \"prop2\": null}}")] // cant supply both even if one is null
        [TestCase("$arg", "ItemWithChildUnion", "{\"prop0\": \"val1\", \"child\":{\"prop1\": \"value1\", \"prop2\": 15}}")] // cant supply both
        [TestCase("$arg", "ItemWithChildUnion", "{\"prop0\": \"val1\", \"child\":{\"prop1\": null, \"prop2\": 15}}")] // cant supply both even if other is null
        [TestCase("$arg", "ItemWithChildUnion", "{\"prop0\": \"val1\", \"child\":{\"prop1\": \"value\", \"notAProp\": 15}}")] // cant supply invalid props
        [TestCase("$arg", "ItemWithChildUnion", "{\"prop0\": \"val1\", \"child\":{\"prop1\": \"value\", \"notAProp\": null}}")] // cant supply invalid props even when null
        [TestCase("$arg", "ItemWithChildUnion", "{\"prop0\": \"val1\", \"child\":{\"notAProp\": \"value\", \"notAProp2\" : 15}}")] // cant supply just invalid props
        public async Task ArgumentWithChildInputUnion_VariableData_WhenSupplyingUncoercableData_RejectsQuery(
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
                .AddQueryText("query ($arg: " + variableDeclaration + "){submitSingleValueWithChildUnion(input: " + inputDeclaration + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.False);
            Assert.That(result.Messages.Count, Is.EqualTo(1));

            // must only be one rule (the rule specific to @oneOf)
            var msg = result.Messages.Single();
            Assert.That(msg.Severity, Is.EqualTo(GraphMessageSeverity.Critical));
            Assert.That(msg.MetaData.ContainsKey("Rule"), Is.True);
            Assert.That(msg.MetaData["Rule"].ToString(), Is.EqualTo("3.10.1"));
        }

        [TestCase("[{prop0: \"val0\", child: {prop1: \"value1\"}}]")]
        [TestCase("[{prop0: \"val0\", child: {prop2: 15}}]")]
        [TestCase("[{prop0: \"val0\", child: {prop2: 15}},{prop0: \"val0\", child: {prop1: \"value1\"}}]")]
        [TestCase("[{prop0: \"val0\", child: {prop2: 15}},null, {prop0: \"val0\", child: {prop1: \"value1\"}}]")] // null should be successfully skipped
        [TestCase("[]")] // empty set is fine. the @oneOf directive applies to the items in the list
        public async Task ArgumentAsListOfChildInputUnions_ObjectLiteral_WhenSupplyingCoeracbleData_IsSuccessful(string argumentValue)
        {
            var server = new TestServerBuilder()
                .AddType<OneOfDirectiveController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query { submitListOfValuesWithChildUnion(inputs: " + argumentValue + ")}");

            var expectedResponse = @"
            {
                ""data"" : {
                    ""submitListOfValuesWithChildUnion"":  ""success""
                }
            }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedResponse,
                result);
        }

        [TestCase("[{prop0: \"val0\", child: {prop1: \"value\", prop2: 13}}]")] // cant supply both
        [TestCase("[{prop0: \"val0\", child: {prop1: \"value\", prop2: null}}]")] // cant supply both even if one is null
        [TestCase("[{prop0: \"val0\", child: {prop1: null, prop2: 13}}]")] // cant supply both even if 'the other' is null
        [TestCase("[{prop0: \"val0\", child: {prop1: null, prop2: null}}]")] // cant supply both as null
        [TestCase("[{prop0: \"val0\", child: {prop2: 15}},{prop0: \"val0\", child: {prop1: null, prop2: null}}]")] // one in the list is wrong.
        [TestCase("[{prop0: \"val0\", child: {prop1: null}}]")] // cant supply just one prop even if supplied as null
        [TestCase("[{prop0: \"val0\", child: {}}]")] // can't supply nothing
        public async Task ArgumentAsListOfChildInputUnions_ObjectLiteral_WhenSupplyingUncoeracbleData_RejectsQuery(string inputDeclaration)
        {
            var server = new TestServerBuilder()
                .AddType<OneOfDirectiveController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query {submitListOfValuesWithChildUnion(inputs: " + inputDeclaration + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.False);
            Assert.That(result.Messages.Count, Is.EqualTo(1));

            // must only be one rule (the rule specific to @oneOf)
            var msg = result.Messages.Single();
            Assert.That(msg.Severity, Is.EqualTo(GraphMessageSeverity.Critical));
            Assert.That(msg.MetaData["Rule"].ToString(), Is.EqualTo("3.10.1"));
        }

        [TestCase("[{prop0:\"val0\", child: {prop1: $arg }}]", "String", "\"someValue\"")] // if the value ont he prop of a union in a list is not null, we're good
        [TestCase("[{prop0:\"val0\", child: {prop2: $arg }}]", "Int!", "15")] // if the value ont he prop of a union is not null, we're good
        [TestCase("[{prop0:\"val0\", child: $arg}]", "MyInputUnion", "{\"prop1\": \"someVal\"}")]
        [TestCase("[{prop0:\"val0\", child: $arg}]", "MyInputUnion", "{\"prop2\": 15}")]
        [TestCase("[{prop0:\"val0\", child: $arg}]", "MyInputUnion", "null")]
        [TestCase("$arg", "[ItemWithChildUnion]", "[{\"prop0\":\"val0\", \"child\": {\"prop2\": 15}}]")] // list with a valid object with a single prop
        [TestCase("$arg", "[ItemWithChildUnion]", "[{\"prop0\":\"val0\", \"child\": {\"prop2\": 15}}, null, {\"prop0\":\"val0\", \"child\": null}, {\"prop0\":\"val0\", \"child\":{\"prop1\": \"strvalue\"}}]")] // a list with multiple valid objects and a null
        [TestCase("$arg", "[ItemWithChildUnion]", "[]")] // an empty list is valid, no objects is fine
        [TestCase("$arg", "[ItemWithChildUnion]", "null")] // we can supply straight up null, since the type expression allows it in this case
        public async Task ArgumentAsListOfChildInputUnions_VariableData_WhenSupplyingCoercableData_IsSuccessful(
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
                .AddQueryText("query ($arg: " + variableDeclaration + "){submitListOfValuesWithChildUnion(inputs: " + inputDeclaration + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.True);
            Assert.That(result.Messages.Count, Is.EqualTo(0));
        }

        [TestCase("[{prop0:\"val0\", child:{prop1: $arg }}]", "String", "null")] // if the value ont he prop of a union in a list is null
        [TestCase("[{prop0:\"val0\", child:{prop2: $arg }}]", "Int", "null")] // if the value ont he prop of a union in a list is null
        [TestCase("$arg", "[ItemWithChildUnion]", "[{\"prop0\":\"val0\", \"child\":{\"prop2\": null}}]")] // list with an object that has an a nulled field
        [TestCase("$arg", "[ItemWithChildUnion]", "[{\"prop0\":\"val0\", \"child\":{\"prop2\": 15, \"prop1\": \"someVal\"}}]")] // list with an object that has more than one field
        [TestCase("$arg", "[ItemWithChildUnion]", "[{\"prop0\":\"val0\", \"child\":{\"prop2\": 15, \"prop1\": null}}]")] // list with an object that has more than one field and one is null
        [TestCase("$arg", "[ItemWithChildUnion]", "[{\"prop0\":\"val0\", \"child\":{}}]")] // list with an object that has no fields
        [TestCase("$arg", "[ItemWithChildUnion]", "[{\"prop0\":\"val0\", \"child\":{\"prop2\": 15}}, {\"prop0\":\"val0\",\"child\": null}, {\"prop0\":\"val0\", \"child\":{\"prop1\": null}}]")] // a list with multiple valid objects and an invalid

        // as a union that is on an item in a list
        [TestCase("[{prop0:\"val0\", child: $arg}]", "MyInputUnion", "{}")] // if the value ont he prop of a union in a list is null
        [TestCase("[{prop0:\"val0\", child: $arg}]", "MyInputUnion", "{\"prop2\": null}")] // if the value ont he prop of a union in a list is null
        [TestCase("[{prop0:\"val0\", child: $arg}]", "MyInputUnion", "{\"prop1\": null}")] // if the value ont he prop of a union in a list is null
        [TestCase("[{prop0:\"val0\", child: $arg}]", "MyInputUnion", "{\"prop1\": \"someVal\", \"prop2\": null}")] // if the value ont he prop of a union in a list is null
        [TestCase("[{prop0:\"val0\", child: $arg}]", "MyInputUnion", "{\"prop1\": \"someVal\", \"prop2\": 15}")] // if the value ont he prop of a union in a list is null
        public async Task ArgumentAsListOfChildInputUnions_VariableData_WhenSupplyingUncoercableData_RejectsQuery(
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
                .AddQueryText("query ($arg: " + variableDeclaration + "){submitListOfValuesWithChildUnion(inputs: " + inputDeclaration + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.False);
            Assert.That(result.Messages.Count, Is.EqualTo(1));

            // must only be one rule (the rule specific to @oneOf)
            var msg = result.Messages.Single();
            Assert.That(msg.Severity, Is.EqualTo(GraphMessageSeverity.Critical));
            Assert.That(msg.MetaData.ContainsKey("Rule"), Is.True);
            Assert.That(msg.MetaData["Rule"].ToString(), Is.EqualTo("3.10.1"));
        }

        [TestCase("{prop: \"stringPropValue\"}")] // child union prop isn't supplied, should be fine
        [TestCase("{child: {prop1: 15 }}")] // only child prop with valid child (also one prop) is fine
        [TestCase("{child: {prop0: \"val0\" }}")] // only child prop with valid child (also one prop) is fine
        public async Task NestedUnion_ObjectLiteral_WhenDataIsCoercable_IsSuccessful(string inputValue)
        {
            // nesting unions!
            var server = new TestServerBuilder()
                .AddType<OneOfDirectiveNestingController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query{ unionInUnion(input: " + inputValue + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.True);
            Assert.That(result.Messages.Count, Is.EqualTo(0));
        }

        [TestCase("{prop: \"stringPropValue\", child: {prop1: 15}}")] // cant supply both to outer union
        [TestCase("{child: {prop1: null }}")] // invalid value on inner union
        [TestCase("{child: {prop0: null }}")] // invalid value on inner union
        [TestCase("{child: {}}")] // cant supply nothing on inner union
        [TestCase("{}")] // cant supply nothing on outer union
        public async Task NestedUnion_ObjectLiteral_WhenDataIsUnCoercable_RejectsQuery(string inputValue)
        {
            // nesting unions!
            var server = new TestServerBuilder()
                .AddType<OneOfDirectiveNestingController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query{ unionInUnion(input: " + inputValue + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.False);
            Assert.That(result.Messages.Count, Is.EqualTo(1));

            // must only be one rule (the rule specific to @oneOf)
            var msg = result.Messages.Single();
            Assert.That(msg.Severity, Is.EqualTo(GraphMessageSeverity.Critical));
            Assert.That(msg.MetaData.ContainsKey("Rule"), Is.True);
            Assert.That(msg.MetaData["Rule"].ToString(), Is.EqualTo("3.10.1"));
        }

        [TestCase("{prop: \"stringPropValue\"}")] // outer union, no middle union
        [TestCase("{child: {prop: \"val\"} }")] // outer union, middle union, no inner union
        [TestCase("{child: { child : {prop1: 15} } }")] // unions all teh way down
        public async Task TripleNestedUnion_ObjectLiteral_WhenDataIsCoercable_IsSuccessful(string inputValue)
        {
            // nesting unions!
            var server = new TestServerBuilder()
                .AddType<OneOfDirectiveNestingController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query{ tripleNestedUnion(input: " + inputValue + ")}");

            var result = await server.ExecuteQuery(builder);

            Assert.That(result.Messages.IsSucessful, Is.True);
            Assert.That(result.Messages.Count, Is.EqualTo(0));
        }

        // Invalid outer union
        [TestCase("{}")]
        [TestCase("{prop: \"val\", child: {prop: \"val\"}}")]
        [TestCase("{prop: null, child: {prop: \"val\"}}")]
        [TestCase("{prop: null}")]
        [TestCase("{child: null}")]

        // invalid middle union
        [TestCase("{child: {}}")]
        [TestCase("{child: {prop: \"val\", child: {prop: \"val\"} }}")]
        [TestCase("{child: {prop: null, child: {prop: \"val\"} }}")]
        [TestCase("{child: {prop: null, child: null }}")]
        [TestCase("{child: {prop: null }}")]
        [TestCase("{child: {child: null }}")]

        // invalid inner union
        [TestCase("{child: {child: {} }}")]
        [TestCase("{child: {child: {prop0: \"val\", prop1: 15} }}")]
        [TestCase("{child: {child: {prop0: \"val\", prop1: null} }}")]
        [TestCase("{child: {child: {prop0: null, prop1: 15} }}")]
        [TestCase("{child: {child: {prop0: null, prop1: null} }}")]
        [TestCase("{child: {child: {prop0: null} }}")]
        [TestCase("{child: {child: {prop1: null} }}")]

        // give it everything! mega wrong
        [TestCase("{prop: \"val\", child: {prop: \"val\", child: {prop0: \"val\", prop1: 15} } }")]
        public async Task TripleNestedUnion_ObjectLiteral_WhenDataIsUnCoercable_RejectsQuery(string inputValue)
        {
            // nesting unions!
            var server = new TestServerBuilder()
                .AddType<OneOfDirectiveNestingController>()
                .AddType<OneOfDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query{ tripleNestedUnion(input: " + inputValue + ")}");

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