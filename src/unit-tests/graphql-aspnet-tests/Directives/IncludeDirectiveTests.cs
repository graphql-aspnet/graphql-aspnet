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
    internal class IncludeDirectiveTests
    {
        [Test]
        public async Task IncludeDirectiveWithFalse_OnField_FieldIsNotIncluded()
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
                                property2 @include(if: false)
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
        public async Task IncludeDirectiveWithTrue_OnField_FieldIsIncluded()
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
                                property2 @include(if: true)
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
        public async Task IncludeDirectiveWithFalse_OnInlineFragment_FieldIsNotIncluded()
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
                                ... @include(if: false) {
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
        public async Task IncludeDirectiveWithTrue_OnInlineFragment_FieldIsIncluded()
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
                                ... @include(if: true) {
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
        public async Task IncludeDirectiveWithFalse_OnFragmentSpread_FieldIsNotIncluded()
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
                                ... frag1 @include(if: false)
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

        [Test]
        public async Task IncludeDirectiveWithTrue_OnFragmentSpread_FieldIsIncluded()
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
                                ... frag1 @include(if: true)
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
    }
}