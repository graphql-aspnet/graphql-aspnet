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
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Directives.DirectiveTestData;
    using GraphQL.AspNet.Tests.Framework;
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

        [Test]
        public async Task SkipDirectiveOnInlineFragment_DropsAllFields_YieldsError_5_3_3()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                .Build();

            // result is no leaf fields on simpleQueryMethod
            // violate rule 5.3.3 (leaf field selections)
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                ... @skip(if: true) {
                                    property1,
                                    property2
                                }
                            }
                        }
                    }");

            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual("5.3.3", result.Messages[0].MetaData[Constants.Messaging.REFERENCE_RULE_NUMBER_KEY]);
        }

        [Test]
        public async Task SkipDirectiveOnFragmentSpread_DropsAllFields_YieldsError_5_3_3()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                .Build();

            // result is no leaf fields on simpleQueryMethod
            // violate rule 5.3.3 (leaf field selections)
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                ... frag1 @skip(if: true)
                            }
                        }
                    }

                    fragment frag1 on TwoPropertyObject {
                        property1
                        property2
                    }");

            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual("5.3.3", result.Messages[0].MetaData[Constants.Messaging.REFERENCE_RULE_NUMBER_KEY]);
        }

        [Test]
        public async Task SkipDirectiveIsNotRepeatable()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                property1
                                property2 @skip(if: true) @skip(if: false)
                            }
                        }
                    }");

            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(Constants.ErrorCodes.INVALID_DOCUMENT, result.Messages[0].Code);
        }
    }
}