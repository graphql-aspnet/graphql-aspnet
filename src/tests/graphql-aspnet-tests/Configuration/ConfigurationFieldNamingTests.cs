// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Configuration
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration.Formatting;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Configuration.ConfigurationTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class ConfigurationFieldNamingTests
    {
        [Test]
        public async Task ProductionDefaults_NoChanges()
        {
            var builder = new TestServerBuilder();
            var server = builder.AddGraphType<FanController>()
                .Build();
            var queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText("{ retrieveFan(name: \"bob\"){ id, name, fanSpeed} }");

            var result = await server.RenderResult(queryBuilder).ConfigureAwait(false);

            var expectedResult = @"
            {
                ""data"" : {
                    ""retrieveFan"": {
                        ""id"" : 1,
                        ""name"" : ""bob"",
                        ""fanSpeed"" : ""MEDIUM""
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
        }

        [Test]
        public async Task ProductionDefaults_UpperCaseFieldNames()
        {
            var builder = new TestServerBuilder()
                            .AddGraphType<FanController>();

            builder.AddGraphQL(o =>
            {
                o.DeclarationOptions.GraphNamingFormatter = new GraphNameFormatter(fieldNameStrategy: GraphNameFormatStrategy.UpperCase);
            });

            var server = builder.Build();

            var queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText("{ RETRIEVEFAN(NAME: \"bob\"){ ID, NAME, FANSPEED} }");

            var result = await server.RenderResult(queryBuilder).ConfigureAwait(false);

            var expectedResult = @"
            {
                ""data"" : {
                    ""RETRIEVEFAN"": {
                        ""ID"" : 1,
                        ""NAME"" : ""bob"",
                        ""FANSPEED"" : ""MEDIUM""
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
        }

        [Test]
        public async Task ProductionDefaults_LowerCaseEnumValues()
        {
            var builder = new TestServerBuilder()
                        .AddGraphType<FanController>();

            builder.AddGraphQL(o =>
            {
                o.DeclarationOptions.GraphNamingFormatter = new GraphNameFormatter(enumValueStrategy: GraphNameFormatStrategy.LowerCase);
            });

            var server = builder.Build();

            var queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText("{ retrieveFan(name: \"bob\"){ id, name, fanSpeed} }");

            var result = await server.RenderResult(queryBuilder).ConfigureAwait(false);

            var expectedResult = @"
            {
                ""data"" : {
                    ""retrieveFan"": {
                        ""id"" : 1,
                        ""name"" : ""bob"",
                        ""fanSpeed"" : ""medium""
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
        }

        [Test]
        public async Task ProductionDefaults_CamelCasedTypeNames()
        {
            var builder = new TestServerBuilder()
                        .AddGraphType<FanController>();

            builder.AddGraphQL(o =>
            {
                o.DeclarationOptions.GraphNamingFormatter = new GraphNameFormatter(typeNameStrategy: GraphNameFormatStrategy.CamelCase);
            });

            var server = builder.Build();

            var queryBuilder = server.CreateQueryContextBuilder();
            queryBuilder.AddQueryText("{ __type(name: \"fanItem\"){ name, kind} }");

            var result = await server.RenderResult(queryBuilder).ConfigureAwait(false);

            var expectedResult = @"
            {
                ""data"" : {
                    ""__type"": {
                        ""name"" : ""fanItem"",
                        ""kind"" : ""OBJECT"",
                    }
                }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResult, result);
        }
    }
}