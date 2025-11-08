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
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Execution.TestData.IntrospectionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class IntrospectionIsOneOfFieldTests
    {
        [Test]
        public async Task OneOfInputUnion_SetsIsOneOfCorrectly()
        {
            var server = new TestServerBuilder()
                .AddType<InputUnionWithOneOfForIntrospection>(TypeKind.INPUT_OBJECT)
                .Build();

            var context = server.CreateQueryContextBuilder()
                .AddQueryText(@"
                {
                   #custom named
                    __type(name: ""OneOfForIntrospection"")
                                    {
                                        kind
                                        name
                                        isOneOf
                                        inputFields {
                                            name
                                        }
                                    }
                }");

            var response = await server.RenderResult(context);

            var expectedResult = @"
            {
              ""data"": {
                ""__type"": {
                  ""kind"": ""INPUT_OBJECT"",
                  ""name"": ""OneOfForIntrospection"",
                  ""isOneOf"": true,
                  ""inputFields"": [
                    {
                      ""name"": ""prop1""
                    },
                    {
                      ""name"": ""prop2""
                    }
                  ]
                }
              }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, response);
        }

        [Test]
        public async Task OneOfInputUnion_WhenUsedAsRegularObject_DoesNotSetOneOfOnIntrospection()
        {
            var server = new TestServerBuilder()
                .AddType<InputUnionWithOneOfForIntrospection>(TypeKind.OBJECT)
                .Build();

            var context = server.CreateQueryContextBuilder()
                .AddQueryText(@"
                {
                   #custom named
                    __type(name: ""InputUnionWithOneOfForIntrospection"")
                                    {
                                        kind
                                        name
                                        isOneOf
                                    }
                }");

            var response = await server.RenderResult(context);

            var expectedResult = @"
            {
              ""data"": {
                ""__type"": {
                  ""kind"": ""OBJECT"",
                  ""name"": ""InputUnionWithOneOfForIntrospection"",
                  ""isOneOf"": false
                }
              }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, response);
        }

        [Test]
        public async Task Interface_DoesNotSetIsOneOfToTrue()
        {
            var server = new TestServerBuilder()
                .AddType<ISodaType>()
                .Build();

            var context = server.CreateQueryContextBuilder()
                .AddQueryText(@"
                {
                   #custom named
                    __type(name: ""ISodaType"")
                                    {
                                        kind
                                        name
                                        isOneOf
                                    }
                }");

            var response = await server.RenderResult(context);

            var expectedResult = @"
            {
              ""data"": {
                ""__type"": {
                  ""kind"": ""INTERFACE"",
                  ""name"": ""ISodaType"",
                  ""isOneOf"": false
                }
              }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, response);
        }

        [Test]
        public async Task Enum_DoesNotSetIsOneOfToTrue()
        {
            var server = new TestServerBuilder()
                .AddType<IntrospectableEnum>()
                .Build();

            var context = server.CreateQueryContextBuilder()
                .AddQueryText(@"
                {
                   #custom named
                    __type(name: ""IntrospectableEnum"")
                                    {
                                        kind
                                        name
                                        isOneOf
                                    }
                }");

            var response = await server.RenderResult(context);

            var expectedResult = @"
            {
              ""data"": {
                ""__type"": {
                  ""kind"": ""ENUM"",
                  ""name"": ""IntrospectableEnum"",
                  ""isOneOf"": false
                }
              }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, response);
        }
    }
}