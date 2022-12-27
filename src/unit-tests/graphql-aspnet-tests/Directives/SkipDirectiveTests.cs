// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Directives
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Tests.Directives.DirectiveTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class SkipDirectiveTests
    {
        [Test]
        public async Task SkipDirectiveWithFalse_OnField_FieldIsIncluded()
        {
            var server = new TestServerBuilder()
            .AddType<SimpleExecutionController>()
                .AddType<SkipDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {  
                        simple {  
                            simpleQueryMethod { 
                                property1, 
                                property2 @skip(if: false)
                            }
                        }
                    }");

            var expectedResponse = @"
            {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"" : {
                                ""property1"" : ""default string"",
                                ""property2"": 5
                        }
                    }
                }
            }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedResponse,
                result);
        }

        [Test]
        public async Task SkipDirectiveWithTrue_OnField_FieldIsNotIncluded()
        {
            var server = new TestServerBuilder()
            .AddType<SimpleExecutionController>()
                .AddType<SkipDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {  
                        simple {  
                            simpleQueryMethod { 
                                property1, 
                                property2 @skip(if: true)
                            }
                        }
                    }");

            var expectedResponse = @"
            {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"" : {
                                ""property1"" : ""default string""
                        }
                    }
                }
            }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedResponse,
                result);
        }

        [Test]
        public async Task SkipDirectiveWithFalse_OnInlineFragment_FieldIsIncluded()
        {
            var server = new TestServerBuilder()
            .AddType<SimpleExecutionController>()
                .AddType<SkipDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                property1,
                                ... @skip(if: false) {
                                    property2
                                }
                            }
                        }
                    }");

            var expectedResponse = @"
            {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"" : {
                                ""property1"" : ""default string"",
                                ""property2"": 5
                        }
                    }
                }
            }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedResponse,
                result);
        }

        [Test]
        public async Task SkipDirectiveWithTrue_OnInlineFragment_FieldIsNotIncluded()
        {
            var server = new TestServerBuilder()
            .AddType<SimpleExecutionController>()
                .AddType<SkipDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                property1,
                                ... @skip(if: true) {
                                    property2
                                }
                            }
                        }
                    }");

            var expectedResponse = @"
            {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"" : {
                                ""property1"" : ""default string""
                        }
                    }
                }
            }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedResponse,
                result);
        }



        [Test]
        public async Task SkipDirectiveWithFalse_OnFragmentSpread_FieldIsIncluded()
        {
            var server = new TestServerBuilder()
            .AddType<SimpleExecutionController>()
                .AddType<SkipDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                property1,
                                ... frag1 @skip(if: false)
                            }
                        }
                    }
                    fragment frag1 on TwoPropertyObject {
                        property2
                    }");

            var expectedResponse = @"
            {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"" : {
                                ""property1"" : ""default string"",
                                ""property2"": 5
                        }
                    }
                }
            }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedResponse,
                result);
        }

        [Test]
        public async Task SkipDirectiveWithTrue_OnFragmentSpread_FieldIsNotIncluded()
        {
            var server = new TestServerBuilder()
            .AddType<SimpleExecutionController>()
                .AddType<SkipDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                property1,
                                ... frag1 @skip(if: true)
                            }
                        }
                    }
                    fragment frag1 on TwoPropertyObject {
                        property2
                    }");

            var expectedResponse = @"
            {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"" : {
                                ""property1"" : ""default string""
                        }
                    }
                }
            }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedResponse,
                result);
        }
    }
}