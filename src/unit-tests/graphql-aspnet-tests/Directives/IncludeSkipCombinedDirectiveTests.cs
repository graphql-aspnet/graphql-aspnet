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
    public class IncludeSkipCombinedDirectiveTests
    {
        [TestCase(true, false, true)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, true, false)]
        public async Task IncludeDirective_ThenSkipDirective_OnSingleField(bool includeIf, bool skipIf, bool fieldShouldBeIncluded)
        {
            var server = new TestServerBuilder()
         .AddType<SimpleExecutionController>()
             .AddType<IncludeDirective>()
             .AddType<SkipDirective>()
             .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    "query Operation1{  simple {  simpleQueryMethod { property1, property2 " +
                    "@include(if: " + includeIf.ToString().ToLower() + ") " +
                    "@skip(if: " + skipIf.ToString().ToLower() + ")" +
                    "} } }")
                .AddOperationName("Operation1");

            var expectedJson = @"
            {
            ""data"" : {
                ""simple"": {
                  ""simpleQueryMethod"": {
                    ""property1"": ""default string""" +
                    (fieldShouldBeIncluded ? ",\"property2\" : 5" : string.Empty) +
            @"
                  }
                }
              }
            }
            ";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [TestCase(true, false, true)]
        [TestCase(false, true, false)]
        [TestCase(false, false, false)]
        [TestCase(true, true, false)]
        public async Task SkipDirective_ThenIncludeDirective_OnSingleField(bool includeIf, bool skipIf, bool fieldShouldBeIncluded)
        {
            var server = new TestServerBuilder()
         .AddType<SimpleExecutionController>()
             .AddType<IncludeDirective>()
             .AddType<SkipDirective>()
             .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    "query Operation1{  simple {  simpleQueryMethod { property1, property2 " +
                    "@skip(if: " + skipIf.ToString().ToLower() + ")" +
                    "@include(if: " + includeIf.ToString().ToLower() + ") " +
                    "} } }")
                .AddOperationName("Operation1");

            var expectedJson = @"
            {
            ""data"" : {
                ""simple"": {
                  ""simpleQueryMethod"": {
                    ""property1"": ""default string""" +
                    (fieldShouldBeIncluded ? ",\"property2\" : 5" : string.Empty) +
            @"
                  }
                }
              }
            }
            ";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [TestCase("@skip(if:false) @include(if: true)", false)]
        [TestCase("@skip(if:false) @include(if: false)", true)]
        [TestCase("@skip(if:true) @include(if: true)", true)]
        [TestCase("@skip(if:true) @include(if: false)", true)]
        [TestCase("@include(if: true) @skip(if:false)", false)]
        [TestCase("@include(if: false) @skip(if:false)", true)]
        [TestCase("@include(if: true) @skip(if:true)", true)]
        [TestCase("@include(if: false) @skip(if:true)", true)]
        public async Task SkipDirective_AndIncludeDirective_OnInlineFragment_YieldsData(string directives, bool isError)
        {
            var server = new TestServerBuilder()
         .AddType<SimpleExecutionController>()
             .AddType<IncludeDirective>()
             .AddType<SkipDirective>()
             .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                ... " + directives + @" {
                                    property1,
                                    property2
                                }
                            }
                        }
                    }");

            if (!isError)
            {
                var expectedJson = @"
            {
            ""data"" : {
                ""simple"": {
                  ""simpleQueryMethod"": {
                    ""property1"": ""default string"",
                    ""property2"" : 5,
                  }
                }
              }
            }";

                var result = await server.RenderResult(builder);
                CommonAssertions.AreEqualJsonStrings(
                    expectedJson,
                    result);
            }
            else
            {
                var result = await server.ExecuteQuery(builder);
                Assert.AreEqual(1, result.Messages.Count);
                Assert.AreEqual("5.3.3", result.Messages[0].MetaData[Constants.Messaging.REFERENCE_RULE_NUMBER_KEY]);
            }
        }

        [TestCase("@skip(if:false) @include(if: true)", false)]
        [TestCase("@skip(if:false) @include(if: false)", true)]
        [TestCase("@skip(if:true) @include(if: true)", true)]
        [TestCase("@skip(if:true) @include(if: false)", true)]
        [TestCase("@include(if: true) @skip(if:false)", false)]
        [TestCase("@include(if: false) @skip(if:false)", true)]
        [TestCase("@include(if: true) @skip(if:true)", true)]
        [TestCase("@include(if: false) @skip(if:true)", true)]
        public async Task SkipDirective_AndIncludeDirective_OnFragmentSpread_YieldsData(string directives, bool isError)
        {
            var server = new TestServerBuilder()
         .AddType<SimpleExecutionController>()
             .AddType<IncludeDirective>()
             .AddType<SkipDirective>()
             .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                ... frag1 " + directives + @"
                            }
                        }
                    }
                    fragment frag1 on TwoPropertyObject {
                        property1
                        property2
                    }");

            if (!isError)
            {
                var expectedJson = @"
                {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"": {
                        ""property1"": ""default string"",
                        ""property2"" : 5,
                        }
                    }
                    }
                }";

                var result = await server.RenderResult(builder);
                CommonAssertions.AreEqualJsonStrings(
                    expectedJson,
                    result);
            }
            else
            {
                var result = await server.ExecuteQuery(builder);
                Assert.AreEqual(1, result.Messages.Count);
                Assert.AreEqual("5.3.3", result.Messages[0].MetaData[Constants.Messaging.REFERENCE_RULE_NUMBER_KEY]);
            }
        }

        [Test]
        public async Task NestedDirectives_Scenario1()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                 .AddType<SkipDirective>()
                 .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                ... frag1 @include(if: true)
                            }
                        }
                    }
                    fragment frag1 on TwoPropertyObject {
                        property1 @skip(if: true)
                        property2
                    }");

            var expectedJson = @"
                {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"": {
                        ""property2"" : 5,
                       }
                     }
                   }
                }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [Test]
        public async Task NestedDirectives_Scenario2()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                 .AddType<SkipDirective>()
                 .Build();

            // even though property1 is skipped on the fragment its still directly
            // included and thus is included
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                property1
                                ... frag1 @include(if: true)
                            }
                        }
                    }
                    fragment frag1 on TwoPropertyObject {
                        property1 @skip(if: true)
                        property2
                    }");

            var expectedJson = @"
                {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"": {
                        ""property1"" : ""default string"",
                        ""property2"" : 5,
                       }
                     }
                   }
                }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [Test]
        public async Task NestedDirectives_Scenario3()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                 .AddType<SkipDirective>()
                 .Build();

            // Fragment is included
            // property1 is skipped on fragment
            // property1 is included via inline fragment
            // merged set includes property1
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                ... frag1 @include(if: true)
                            }
                        }
                    }
                    fragment frag1 on TwoPropertyObject {
                        property1 @skip(if: true)
                        property2
                        ... @include(if: true) {
                            property1
                        }
                    }");

            var expectedJson = @"
                {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"": {
                        ""property1"" : ""default string"",
                        ""property2"" : 5,
                       }
                     }
                   }
                }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [Test]
        public async Task NestedDirectives_Scenario4()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                 .AddType<SkipDirective>()
                 .Build();

            // Fragment is included
            // property1 is skipped on fragment
            // property1 is skipped via inline fragment
            // merged set does not include property1
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                ... frag1 @include(if: true)
                            }
                        }
                    }
                    fragment frag1 on TwoPropertyObject {
                        property1 @skip(if: true)
                        property2
                        ... @include(if: true) {
                            property1 @skip(if: true)
                        }
                    }");

            var expectedJson = @"
                {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"": {
                        ""property2"" : 5,
                       }
                     }
                   }
                }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [Test]
        public async Task NestedDirectives_Scenario5()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                 .AddType<SkipDirective>()
                 .Build();

            // the first @include(if: true) would garuntee property1
            // to be included in the selection set, regardless of others
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                property2
                                ... @include(if: true) {
                                    property1
                                }
                                ... @skip(if: true) {
                                    property1
                                }
                                ... @include(if: false) {
                                    property1
                                }
                                ... @skip(if: true) {
                                    property1
                                }
                                ... @include(if: false) {
                                    property1
                                }
                            }
                        }
                    }");

            var expectedJson = @"
                {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"": {
                        ""property2"" : 5,
                        ""property1"" : ""default string""
                       }
                     }
                   }
                }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [Test]
        public async Task NestedDirectives_Scenario6()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                 .AddType<SkipDirective>()
                 .Build();

            // all property1 inline fragments are skipped
            // creates an invalid selections et on simpleQueryMethod
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                ... @include(if: false) {
                                    property1
                                }
                                ... @skip(if: true) {
                                    property1
                                }
                                ... @include(if: false) {
                                    property1
                                }
                                ... @skip(if: true) {
                                    property1
                                }
                                ... @include(if: false) {
                                    property1
                                }
                            }
                        }
                    }");

            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual("5.3.3", result.Messages[0].MetaData[Constants.Messaging.REFERENCE_RULE_NUMBER_KEY]);
        }

        [Test]
        public async Task NestedDirectives_Scenario7()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                 .AddType<SkipDirective>()
                 .Build();

            // property1 is ultimately not included
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                property2
                                ... frag1 @include(if: true)
                            }
                        }
                    }
                    fragment frag1 on TwoPropertyObject {
                        ... @include(if: true) {
                            property1 @include(if: false)
                        }
                    }");

            var expectedJson = @"
                {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"": {
                        ""property2"" : 5
                       }
                     }
                   }
                }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [Test]
        public async Task NestedDirectives_Scenario8()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                 .AddType<SkipDirective>()
                 .Build();

            // super silly but ultimately included!
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                property2
                                ... frag1 @include(if: true)
                            }
                        }
                    }
                    fragment frag1 on TwoPropertyObject {
                        ... @skip(if: false) {
                            ... @include(if: true) {
                                ... @skip(if: false) {
                                    ... @include(if: true) {
                                        ... @skip(if: false) {
                                            ... @include(if: true) {
                                                ... @skip(if: false) {
                                                    ... @include(if: true) {
                                                           property1 @skip(if: false)
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }");

            var expectedJson = @"
                {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"": {
                        ""property2"" : 5,
                        ""property1"" : ""default string""
                       }
                     }
                   }
                }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [Test]
        public async Task NestedDirectives_Scenario9()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                 .AddType<SkipDirective>()
                 .Build();

            // super silly but ultimately valid, field would not be included!
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                property2
                                ... frag1 @include(if: true)
                            }
                        }
                    }
                    fragment frag1 on TwoPropertyObject {
                        ... @skip(if: false) {
                            ... @include(if: true) {
                                ... @skip(if: false) {
                                    ... @include(if: true) {
                                        ... @skip(if: false) {
                                            ... @include(if: true) {
                                                ... @skip(if: false) {
                                                    ... @include(if: true) {
                                                           property1 @skip(if: true)
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }");

            var expectedJson = @"
                {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"": {
                        ""property2"" : 5
                       }
                     }
                   }
                }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [Test]
        public async Task NestedDirectives_Scenario10()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                 .AddType<SkipDirective>()
                 .Build();

            // super silly but ultimately not included!
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                property2
                                ... frag1 @include(if: true)
                            }
                        }
                    }
                    fragment frag1 on TwoPropertyObject {
                        ... @skip(if: false) {
                            ... @include(if: true) {
                                ... @skip(if: false) {
                                    ... @include(if: true) {
                                        ... @skip(if: true) {  # Mid level skipped
                                            ... @include(if: true) {
                                                ... @skip(if: false) {
                                                    ... @include(if: true) {
                                                           property1 @skip(if: false)
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }");

            var expectedJson = @"
                {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"": {
                        ""property2"" : 5
                       }
                     }
                   }
                }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [Test]
        public async Task NestedDirectives_Scenario11()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                 .AddType<SkipDirective>()
                 .Build();

            // super silly + nested fragment
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                property2
                                ... frag1 @include(if: true)
                            }
                        }
                    }
                    fragment frag1 on TwoPropertyObject {
                        ... @skip(if: false) {
                            ... @include(if: true) {
                                ... @skip(if: false) {
                                    ... @include(if: true) {
                                        ... @skip(if: false) {
                                            ... @include(if: true) {
                                                ... @skip(if: false) {
                                                    ... @include(if: true) {
                                                           ... frag2 @include(if: true)
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    fragment frag2 on TwoPropertyObject {
                        property1 @skip(if: false)
                    }");

            var expectedJson = @"
                {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"": {
                        ""property2"" : 5,
                        ""property1"": ""default string""
                       }
                     }
                   }
                }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [Test]
        public async Task NestedDirectives_Scenario12()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                 .AddType<SkipDirective>()
                 .Build();

            // super silly + nested fragment
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                ... frag1 @include(if: true)
                            }
                        }
                    }

                    fragment frag1 on TwoPropertyObject {
                        ... frag2 @include(if: true)
                    }
                    fragment frag2 on TwoPropertyObject {
                        ... frag3 @include(if: true)
                    }
                    fragment frag3 on TwoPropertyObject {
                        ... frag4 @include(if: true)
                    }
                    fragment frag4 on TwoPropertyObject {
                        ... frag5 @skip(if: false)
                        property1 @include(if: true)
                    }
                    fragment frag5 on TwoPropertyObject {
                        ... frag6 @skip(if: false)
                    }
                    fragment frag6 on TwoPropertyObject {
                        property2 @include(if: true)
                    }
                    ");

            var expectedJson = @"
                {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"": {
                        ""property2"" : 5,
                        ""property1"": ""default string""
                       }
                     }
                   }
                }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }

        [Test]
        public async Task NestedDirectives_Scenario13()
        {
            var server = new TestServerBuilder()
                .AddType<SimpleExecutionController>()
                .AddType<IncludeDirective>()
                 .AddType<SkipDirective>()
                 .Build();

            // super silly + nested fragment
            var builder = server.CreateQueryContextBuilder()
                .AddQueryText(
                    @"query {
                        simple {
                            simpleQueryMethod {
                                ... frag1 @include(if: true)
                            }
                        }
                    }

                    fragment frag1 on TwoPropertyObject {
                        ... frag2 @include(if: true)
                    }
                    fragment frag2 on TwoPropertyObject {
                        ... frag3 @include(if: true)
                    }
                    fragment frag3 on TwoPropertyObject {
                        ... frag4 @include(if: true)
                    }
                    fragment frag4 on TwoPropertyObject {
                        ... frag5 @skip(if: false)
                        property1 @include(if: false) # dont include property1
                    }
                    fragment frag5 on TwoPropertyObject {
                        ... frag6 @skip(if: false)
                    }
                    fragment frag6 on TwoPropertyObject {
                        property2 @include(if: true)
                    }
                    ");

            var expectedJson = @"
                {
                ""data"" : {
                    ""simple"": {
                        ""simpleQueryMethod"": {
                        ""property2"" : 5
                       }
                     }
                   }
                }";

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                expectedJson,
                result);
        }
    }
}