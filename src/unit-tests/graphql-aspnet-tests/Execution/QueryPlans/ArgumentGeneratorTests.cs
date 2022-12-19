// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.QueryPlans
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Engine;
    using GraphQL.AspNet.Execution.Parsing;
    using GraphQL.AspNet.Execution.Parsing.Lexing.Source;
    using GraphQL.AspNet.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Execution.QueryPlans.ArgumentGeneratorTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class ArgumentGeneratorTests
    {
        [Test]
        public void FoundArgument_GeneratesResult()
        {
            var server = new TestServerBuilder().AddType<InputController>().Build();
            var text = "query TestQuery{  input {  fetchString(arg1: 5, arg2: 10) } }";

            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(text.AsSpan());

            var queryInputCollection = document.Operations["TestQuery"]
                .FieldSelectionSet.ExecutableFields[0].FieldSelectionSet.ExecutableFields[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchString"].Arguments;

            var arg1 = graphFieldArguments["arg1"];

            var result = argGenerator.CreateInputArgument(arg1);

            Assert.IsNotNull(result);
            Assert.IsTrue((bool)result.IsValid);
            Assert.IsNotNull(result.Argument as ResolvedInputArgumentValue);

            Assert.IsNull(result.Message);
            var data = result.Argument.Resolve(ResolvedVariableCollection.Empty);
            Assert.AreEqual(5, data);
        }

        [Test]
        public void NotFoundArgument_GeneratesResultWithDefaultValue()
        {
            var server = new TestServerBuilder()
                .AddType<InputController>()
                .Build();
            var text = "query TestQuery{  input {  fetchString(arg1: 5) } }";

            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(text.AsSpan());

            var queryInputCollection = document.Operations["TestQuery"].FieldSelectionSet.ExecutableFields[0].FieldSelectionSet.ExecutableFields[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchString"].Arguments;
            var arg2 = graphFieldArguments["arg2"];

            var result = argGenerator.CreateInputArgument(arg2);

            Assert.IsNotNull(result);
            Assert.IsTrue((bool)result.IsValid);
            Assert.IsNotNull(result.Argument as ResolvedInputArgumentValue);
            Assert.IsNull(result.Message);

            // default value on the parameter
            var data = result.Argument.Resolve(ResolvedVariableCollection.Empty);
            Assert.AreEqual(15, data);
        }

        [Test]
        public void FailureToResolve_YieldErrorMessage()
        {
            var server = new TestServerBuilder()
                .AddType<InputController>()
                .Build();

            // set arg1 to int.max + 1; the int graph type will fail to resolve it
            var text = "query TestQuery{  input {  fetchString(arg1: 2147483648 ) } }";

            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(text.AsSpan());

            var queryInputCollection = document.Operations["TestQuery"].FieldSelectionSet.ExecutableFields[0].FieldSelectionSet.ExecutableFields[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchString"].Arguments;
            var arg1 = graphFieldArguments["arg1"];

            var result = argGenerator.CreateInputArgument(arg1);

            Assert.IsNotNull(result);
            Assert.IsFalse((bool)result.IsValid);
            Assert.IsNull(result.Argument as ResolvedInputArgumentValue);

            Assert.IsNotNull(result.Message);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_ARGUMENT, result.Message.Code);
        }

        [Test]
        public void ExpectedNonDeferredList_DoesNotDefer()
        {
            var server = new TestServerBuilder()
                .AddType<InputController>()
                .Build();

            // set arg1 to int.max + 1; the int graph type will fail to resolve it
            var text = "query TestQuery{  input {  fetchArrayTotal(arg3: [1, 2, 3]) } }";

            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(text.AsSpan());

            var queryInputCollection = document.Operations["TestQuery"].FieldSelectionSet.ExecutableFields[0].FieldSelectionSet.ExecutableFields[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchArrayTotal"].Arguments;
            var arg3 = graphFieldArguments["arg3"];

            var result = argGenerator.CreateInputArgument(arg3);

            Assert.IsNotNull(result);
            Assert.IsTrue((bool)result.IsValid);
            Assert.IsNotNull(result.Argument as ResolvedInputArgumentValue);
            Assert.IsNull(result.Message);

            var data = result.Argument.Resolve(ResolvedVariableCollection.Empty) as IEnumerable<int>;
            Assert.IsNotNull(data);

            var expected = new List<int> { 1, 2, 3 };
            CollectionAssert.AreEqual(expected, data);
        }

        [Test]
        public void ExpectedDeferredList_DoesDefer()
        {
            var server = new TestServerBuilder()
                .AddType<InputController>()
                .Build();

            // set arg1 to int.max + 1; the int graph type will fail to resolve it
            var text = "query TestQuery($var1: Int!){  input {  fetchArrayTotal(arg3: [1, $var1, 3]) } }";

            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(text.AsSpan());

            var queryInputCollection = document.Operations["TestQuery"].FieldSelectionSet.ExecutableFields[0].FieldSelectionSet.ExecutableFields[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchArrayTotal"].Arguments;
            var arg3 = graphFieldArguments["arg3"];

            var result = argGenerator.CreateInputArgument(arg3);

            Assert.IsNotNull(result);
            Assert.IsTrue((bool)result.IsValid);
            Assert.IsNotNull(result.Argument as DeferredInputArgumentValue);
            Assert.IsNull(result.Message);
        }

        [Test]
        public void ExpectedNonDeferredComplexObject_DoesNotDefer()
        {
            var server = new TestServerBuilder()
                .AddType<InputController>()
                .Build();

            // set arg1 to int.max + 1; the int graph type will fail to resolve it
            var text = @"
                query TestQuery{
                    input {
                        fetchComplexValue(arg4: {
                                property1: ""bob""
                                property2: 10
                            }
                        )
                    }
                }";

            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(text.AsSpan());

            var queryInputCollection = document.Operations["TestQuery"].FieldSelectionSet.ExecutableFields[0].FieldSelectionSet.ExecutableFields[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchComplexValue"].Arguments;
            var arg3 = graphFieldArguments["arg4"];

            var result = argGenerator.CreateInputArgument(arg3);

            Assert.IsNotNull(result);
            Assert.IsTrue((bool)result.IsValid);
            Assert.IsNotNull(result.Argument as ResolvedInputArgumentValue);
            Assert.IsNull(result.Message);

            var data = result.Argument.Resolve(ResolvedVariableCollection.Empty) as TwoPropertyObject;
            Assert.IsNotNull(data);
            Assert.AreEqual("bob", data.Property1);
            Assert.AreEqual(10, data.Property2);
        }

        [Test]
        public void ExpectedDeferredComplexObject_DoesDefer()
        {
            var server = new TestServerBuilder()
                .AddType<InputController>()
                .Build();

            // set arg1 to int.max + 1; the int graph type will fail to resolve it
            var text = @"
                query TestQuery($var1: Int!){
                    input {
                        fetchComplexValue(arg4: {
                                property1: ""bob""
                                property2: $var1
                            }
                        )
                    }
                }";

            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(text.AsSpan());

            var queryInputCollection = document.Operations["TestQuery"]
                .FieldSelectionSet.ExecutableFields[0].FieldSelectionSet.ExecutableFields[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchComplexValue"].Arguments;
            var arg3 = graphFieldArguments["arg4"];

            var result = argGenerator.CreateInputArgument(arg3);

            Assert.IsNotNull(result);
            Assert.IsTrue((bool)result.IsValid);
            Assert.IsNotNull(result.Argument as DeferredInputArgumentValue);
            Assert.IsNull(result.Message);
        }
    }
}