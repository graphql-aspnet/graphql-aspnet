// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Tests.Execution.TestData.DocumentDescriptionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class DocumentDescriptionTests
    {
        private async Task<string> RenderQuery(string queryText, string variableJsonDoc = null)
        {
            var server = new TestServerBuilder()
                .AddController<DocumentController>()
                .Build();
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(queryText);

            if (!string.IsNullOrWhiteSpace(variableJsonDoc))
                builder.AddVariableData(variableJsonDoc);


            return await server.RenderResult(builder);
        }

        private async Task<IQueryExecutionResult> ExecuteQuery(string queryText, string variableJsonDoc = null)
        {
            var server = new TestServerBuilder()
                .AddController<DocumentController>()
                .Build();
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(queryText);

            if (!string.IsNullOrWhiteSpace(variableJsonDoc))
                builder.AddVariableData(variableJsonDoc);

            return await server.ExecuteQuery(builder);
        }

        [Test]
        public async Task FullQueryDocument_ShouldExecuteCorrectly_WithoutComment()
        {
            var queryText = @"
                query DoTheThing{
                   doThing()
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task FullQueryDocument_WithFragment_ShouldExecuteCorrectly_WithoutComment()
        {
            var queryText = @"
                query DoAThingWithObject{
                   doThingWithObject(){
                       ...objfrag
                   }
                }

                fragment objfrag on TwoPropertyObject {
                     property1
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task FullQueryDocument_ShouldExecuteCorrectly_WhenSuppliedWithASingleLineDescription()
        {
            var queryText = @"
                ""This is a single line query comment full query definition""
                query DoTheThing{
                   doThing()
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task FullQueryDocument_WithFragment_ShouldExecuteCorrectly_WhenSuppliedWithASingleLineDescription()
        {
            var queryText = @"
                ""This is a single line comment on a query""
                query DoAThingWithObject{
                   doThingWithObject(){
                       ...objfrag
                   }
                }

                ""This is a single line comment on a fragment""
                fragment objfrag on TwoPropertyObject {
                     property1
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task FullQueryDocument_ShouldExecuteCorrectly_WhenSuppliedWithAMultilineDescription()
        {
            var queryText = @"
                """"""
                This is a multi line comment with 
                Text on many lines 
                abefore the full query starts
                """"""
                query DoTheThing{
                   doThing()
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task FullQueryDocument_WithFragment_ShouldExecuteCorrectly_WhenSuppliedWithAMultilineDescription()
        {
            var queryText = @"
                """"""
                This is a multi-line comment on a query
                it spans many lines
                """"""
                query DoAThingWithObject{
                   doThingWithObject(){
                       ...objfrag
                   }
                }

                """"""
                This is a multi-line comment on a fragment
                it spans many lines
                """"""
                fragment objfrag on TwoPropertyObject {
                     property1
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task FullQueryDocument_ShouldExecuteQuery_WhenGivenSingleLineVariableDescription()
        {
            var queryText = @"
                query DoAThingWithObject(
                    ""this is an arg1 desc""
                    $arg1: String
                ){
                    doThingWithVars(var1: $arg1, var2: 15)
                    {
                        property1
                        property2
                    }
                }";

            var reslt = await this.ExecuteQuery(queryText, $"{{\"arg1\": \"val1\"}}");
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task FullQueryDocument_ShouldExecuteQuery_WhenSuppliedWithAMultieLineVariableComment()
        {
            var queryText = @"
                query DoAThingWithObject(
                    """"""
                       this is an arg1 desc
                       it spans multiple lines 
                    """"""
                    $arg1: String
                ){
                    doThingWithVars(var1: $arg1, var2: 15)
                    {
                        property1
                        property2
                    }
                }";

            var reslt = await this.ExecuteQuery(queryText, $"{{\"arg1\": \"val1\"}}");
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task ShorthandQueryDocument_ShouldExecuteCorrectly_WithoutComment()
        {
            var queryText = @"
                query{
                   doThing()
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task ShorthandQueryDocument_WithFragment_ShouldExecuteCorrectly_WithoutComment()
        {
            var queryText = @"
                query {
                   doThingWithObject(){
                       ...objfrag
                   }
                }

                fragment objfrag on TwoPropertyObject {
                     property1
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task ShorthandQueryDocument_ShouldBeRejected_WhenSuppliedWithASingleLineDescription()
        {
            var queryText = @"
                ""This is a single line query comment on a shorthand query""
                query {
                     doThing()
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.False);
            Assert.That(reslt.Messages[0].Code, Is.EqualTo(Constants.ErrorCodes.SYNTAX_ERROR));
        }

        [Test]
        public async Task ShorthandQueryDocument_ShouldBeRejected_WhenSuppliedWithAMultilineDescription()
        {
            var queryText = @"
                """"""
                This is a multi line comment with 
                Text on many lines 
                abefore the full query starts
                """"""
                query {
                   doThing()
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.False);
            Assert.That(reslt.Messages[0].Code, Is.EqualTo(Constants.ErrorCodes.SYNTAX_ERROR));
        }

        [Test]
        public async Task ShorthandQueryDocument_WithFragment_ShouldBeRejected_WhenSuppliedWithASingleLineDescription()
        {
            var queryText = @"
                ""This is a single line comment on a query""
                query {
                   doThingWithObject(){
                       ...objfrag
                   }
                }

                ""This is a single line comment on a fragment""
                fragment objfrag on TwoPropertyObject {
                     property1
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.False);
            Assert.That(reslt.Messages[0].Code, Is.EqualTo(Constants.ErrorCodes.SYNTAX_ERROR));
        }

        [Test]
        public async Task ShorthandQueryDocument_WithFragment_ShouldBeRejected_WhenSuppliedWithAMultilineDescription()
        {
            var queryText = @"
                """"""
                This is a multi-line comment on a query
                it spans many lines
                """"""
                query {
                   doThingWithObject(){
                       ...objfrag
                   }
                }

                """"""
                This is a multi-line comment on a fragment
                it spans many lines
                """"""
                fragment objfrag on TwoPropertyObject {
                     property1
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.False);
            Assert.That(reslt.Messages[0].Code, Is.EqualTo(Constants.ErrorCodes.SYNTAX_ERROR));
        }

        [Test]
        public async Task ShorthandQueryDocument_ShouldBeExecuteCorrectly_WhenSuppliedWithASingleLineVariableComment()
        {
            var queryText = @"
                query (
                    ""this is an arg1 desc""
                    $arg1: String
                ){
                    doThingWithVars(var1: $arg1, var2: 15)
                    {
                        property1
                        property2
                    }
                }";

            var reslt = await this.ExecuteQuery(queryText, $"{{\"arg1\": \"val1\"}}");
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task ShorthandQueryDocument_ShouldBeExecuteCorrectly_WhenSuppliedWithAMultieLineVariableComment()
        {
            var queryText = @"
                query (
                    """"""
                       this is an arg1 desc
                       it spans multiple lines
                    """"""
                    $arg1: String
                ){
                    doThingWithVars(var1: $arg1, var2: 15)
                    {
                        property1
                        property2
                    }
                }";

            var reslt = await this.ExecuteQuery(queryText, $"{{\"arg1\": \"val1\"}}");
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task FullQueryDocument_ShouldExecuteCorrectly_WhenSuppliedWithVariableDescriptionAndStringDefaultValue()
        {
            var queryText = @"
                query DoAThingWithObject(
                    ""this is a variable description""
                    $arg1: String = ""default value""
                ){
                    doThingWithVars(var1: $arg1, var2: 15)
                    {
                        property1
                        property2
                    }
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task FullQueryDocument_ShouldExecuteCorrectly_WhenSuppliedWithVariableDescriptionAndMultiLineStringDefaultValue()
        {
            var queryText = @"
                query DoAThingWithObject(
                    """"""
                    Multi-line variable description
                    with details
                    """"""
                    $arg1: String = """"""
                    Multi-line default
                    value string
                    """"""
                ){
                    doThingWithVars(var1: $arg1, var2: 15)
                    {
                        property1
                        property2
                    }
                }";

            var reslt = await this.ExecuteQuery(queryText);
            Assert.That(reslt.Messages.IsSucessful, Is.True);
        }

        [Test]
        public async Task ShorthandQueryDocument_ShouldExecuteCorrectly_WhenSuppliedWithVariableDescriptionAndStringDefaultValue()
        {
            var queryText = @"
                query (
                    ""this is a variable description""
                    $arg1: String = ""default value""
                ){
                    doThingWithVars(var1: $arg1, var2: 15)
                    {
                        property1
                        property2
                    }
                }";

            var reslt = await this.RenderQuery(queryText);

            var expectedResult = @"
                {
                    ""data"": {
                        ""doThingWithVars"" : { 
                            ""property1"": ""default value"",
                            ""property2"": 15
                        }
                    }  
                }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, reslt);
        }

        [Test]
        public async Task ShorthandQueryDocument_ShouldExecuteCorrectly_WhenSuppliedWithVariableDescriptionAndMultiLineStringDefaultValue()
        {
            var queryText = @"
                query (
                    """"""
                    Multi-line variable description
                    with details
                    """"""
                    $arg1: String = """"""
                    Multi-line default
                    value string
                    """"""
                ){
                    doThingWithVars(var1: $arg1, var2: 15)
                    {
                        property1
                        property2
                    }
                }";


            var reslt = await this.RenderQuery(queryText);

            var expectedResult = @"
                {
                    ""data"": {
                        ""doThingWithVars"" : { 
                            ""property1"": ""\n                    Multi-line default\n                    value string\n                    "",
                            ""property2"": 15
                        }
                    }  
                }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, reslt);
        }
    }
}