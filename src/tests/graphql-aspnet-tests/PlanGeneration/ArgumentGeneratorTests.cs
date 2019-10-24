// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.PlanGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mail;
    using GraphQL.AspNet.Defaults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing;
    using GraphQL.AspNet.PlanGeneration.InputArguments;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.PlanGeneration.ArgumentGeneratorTestData;
    using GraphQL.AspNet.Variables;
    using NUnit.Framework;

    [TestFixture]
    public class ArgumentGeneratorTests
    {
        [Test]
        public void FoundArgument_GeneratesResult()
        {
            var server = new TestServerBuilder().AddGraphType<InputController>().Build();

            var parser = new GraphQLParser();
            var syntaxTree = parser.ParseQueryDocument("query TestQuery{  input {  fetchString(arg1: 5, arg2: 10) } }".AsMemory());
            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(syntaxTree);

            var queryInputCollection = document.Operations["TestQuery"].FieldSelectionSet[0].FieldSelectionSet[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchString"].Arguments;

            var arg1 = graphFieldArguments["arg1"];

            var result = argGenerator.CreateInputArgument(arg1);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            Assert.IsNotNull(result.Argument as ResolvedInputArgumentValue);

            Assert.IsNull(result.Message);
            var data = result.Argument.Resolve(ResolvedVariableCollection.Empty);
            Assert.AreEqual(5, data);
        }

        [Test]
        public void NotFoundArgument_GeneratesResultWithDefaultValue()
        {
            var server = new TestServerBuilder()
                .AddGraphType<InputController>()
                .Build();

            var parser = new GraphQLParser();
            var syntaxTree = parser.ParseQueryDocument("query TestQuery{  input {  fetchString(arg1: 5) } }".AsMemory());
            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(syntaxTree);

            var queryInputCollection = document.Operations["TestQuery"].FieldSelectionSet[0].FieldSelectionSet[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchString"].Arguments;
            var arg2 = graphFieldArguments["arg2"];

            var result = argGenerator.CreateInputArgument(arg2);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
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
                .AddGraphType<InputController>()
                .Build();

            var parser = new GraphQLParser();

            // set arg1 to int.max + 1; the int graph type will fail to resolve it
            var syntaxTree = parser.ParseQueryDocument("query TestQuery{  input {  fetchString(arg1: 2147483648 ) } }".AsMemory());
            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(syntaxTree);

            var queryInputCollection = document.Operations["TestQuery"].FieldSelectionSet[0].FieldSelectionSet[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchString"].Arguments;
            var arg1 = graphFieldArguments["arg1"];

            var result = argGenerator.CreateInputArgument(arg1);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);
            Assert.IsNull(result.Argument as ResolvedInputArgumentValue);

            Assert.IsNotNull(result.Message);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_ARGUMENT, result.Message.Code);
        }

        [Test]
        public void ExpectedNonDeferredList_DoesNotDefer()
        {
            var server = new TestServerBuilder()
                .AddGraphType<InputController>()
                .Build();

            var parser = new GraphQLParser();

            // set arg1 to int.max + 1; the int graph type will fail to resolve it
            var syntaxTree = parser.ParseQueryDocument("query TestQuery{  input {  fetchArrayTotal(arg3: [1, 2, 3]) } }".AsMemory());
            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(syntaxTree);

            var queryInputCollection = document.Operations["TestQuery"].FieldSelectionSet[0].FieldSelectionSet[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchArrayTotal"].Arguments;
            var arg3 = graphFieldArguments["arg3"];

            var result = argGenerator.CreateInputArgument(arg3);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
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
                .AddGraphType<InputController>()
                .Build();

            var parser = new GraphQLParser();

            // set arg1 to int.max + 1; the int graph type will fail to resolve it
            var syntaxTree = parser.ParseQueryDocument("query TestQuery($var1: Int!){  input {  fetchArrayTotal(arg3: [1, $var1, 3]) } }".AsMemory());
            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(syntaxTree);

            var queryInputCollection = document.Operations["TestQuery"].FieldSelectionSet[0].FieldSelectionSet[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchArrayTotal"].Arguments;
            var arg3 = graphFieldArguments["arg3"];

            var result = argGenerator.CreateInputArgument(arg3);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            Assert.IsNotNull(result.Argument as DeferredInputArgumentValue);
            Assert.IsNull(result.Message);
        }

        [Test]
        public void ExpectedNonDeferredComplexObject_DoesNotDefer()
        {
            var server = new TestServerBuilder()
                .AddGraphType<InputController>()
                .Build();

            var parser = new GraphQLParser();

            // set arg1 to int.max + 1; the int graph type will fail to resolve it
            var syntaxTree = parser.ParseQueryDocument(@"
                query TestQuery{
                    input {
                        fetchComplexValue(arg4: {
                                property1: ""bob""
                                property2: 10
                            }
                        )
                    }
                }".AsMemory());

            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(syntaxTree);

            var queryInputCollection = document.Operations["TestQuery"].FieldSelectionSet[0].FieldSelectionSet[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchComplexValue"].Arguments;
            var arg3 = graphFieldArguments["arg4"];

            var result = argGenerator.CreateInputArgument(arg3);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
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
                .AddGraphType<InputController>()
                .Build();

            var parser = new GraphQLParser();

            // set arg1 to int.max + 1; the int graph type will fail to resolve it
            var syntaxTree = parser.ParseQueryDocument(@"
                query TestQuery($var1: Int!){
                    input {
                        fetchComplexValue(arg4: {
                                property1: ""bob""
                                property2: $var1
                            }
                        )
                    }
                }".AsMemory());

            var docGenerator = new DefaultGraphQueryDocumentGenerator<GraphSchema>(server.Schema);
            var document = docGenerator.CreateDocument(syntaxTree);

            var queryInputCollection = document.Operations["TestQuery"].FieldSelectionSet[0].FieldSelectionSet[0].Arguments;

            var argGenerator = new ArgumentGenerator(server.Schema, queryInputCollection);

            var schemaType = server.Schema.KnownTypes.FindGraphType("Query_Input");
            var graphFieldArguments = (schemaType as IGraphFieldContainer).Fields["fetchComplexValue"].Arguments;
            var arg3 = graphFieldArguments["arg4"];

            var result = argGenerator.CreateInputArgument(arg3);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            Assert.IsNotNull(result.Argument as DeferredInputArgumentValue);
            Assert.IsNull(result.Message);
        }
    }
}