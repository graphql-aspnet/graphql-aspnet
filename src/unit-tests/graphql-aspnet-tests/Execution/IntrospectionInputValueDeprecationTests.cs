// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Tests.Execution.TestData.IntrospectionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    /// <summary>
    /// Tests for introspection of deprecated input fields and arguments
    /// as per the GraphQL September 2025 specification.
    /// </summary>
    [TestFixture]
    public class IntrospectionInputValueDeprecationTests
    {
        [Test]
        public async Task InputFields_WithIncludeDeprecatedFalse_ExcludesDeprecatedFields()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder
                .AddGraphController<DeprecatedInputFieldsController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                {
                    __type(name: ""DeprecatedFieldsInput"") {
                        kind
                        name
                        inputFields(includeDeprecated: false) {
                            name
                            isDeprecated
                        }
                    }
                }");

            var response = await server.RenderResult(builder);
            var expected = @"
                {
                    ""data"": {
                        ""__type"": {
                            ""kind"": ""INPUT_OBJECT"",
                            ""name"": ""DeprecatedFieldsInput"",
                            ""inputFields"": [
                                { ""name"": ""activeField"", ""isDeprecated"": false },
                                { ""name"": ""count"", ""isDeprecated"": false }
                            ]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public async Task InputFields_WithIncludeDeprecatedTrue_IncludesDeprecatedFields()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder
                .AddGraphController<DeprecatedInputFieldsController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                {
                    __type(name: ""DeprecatedFieldsInput"") {
                        kind
                        name
                        inputFields(includeDeprecated: true) {
                            name
                            isDeprecated
                        }
                    }
                }");

            var response = await server.RenderResult(builder);
            var expected = @"
                {
                    ""data"": {
                        ""__type"": {
                            ""kind"": ""INPUT_OBJECT"",
                            ""name"": ""DeprecatedFieldsInput"",
                            ""inputFields"": [
                                { ""name"": ""activeField"", ""isDeprecated"": false },
                                { ""name"": ""count"", ""isDeprecated"": false },
                                { ""name"": ""legacyField"", ""isDeprecated"": true }
                            ]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public async Task InputFields_DeprecatedField_ExposesDeprecationReason()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder
                .AddGraphController<DeprecatedInputFieldsController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                {
                    __type(name: ""DeprecatedFieldsInput"") {
                        inputFields(includeDeprecated: true) {
                            name
                            isDeprecated
                            deprecationReason
                        }
                    }
                }");

            var response = await server.RenderResult(builder);
            var expected = @"
                {
                    ""data"": {
                        ""__type"": {
                            ""inputFields"": [
                                { ""name"": ""activeField"", ""isDeprecated"": false, ""deprecationReason"": null },
                                { ""name"": ""count"", ""isDeprecated"": false, ""deprecationReason"": null },
                                { ""name"": ""legacyField"", ""isDeprecated"": true, ""deprecationReason"": ""Use activeField instead"" }
                            ]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public async Task InputFields_DefaultBehavior_ExcludesDeprecatedFields()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder
                .AddGraphController<DeprecatedInputFieldsController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                {
                    __type(name: ""DeprecatedFieldsInput"") {
                        inputFields {
                            name
                            isDeprecated
                        }
                    }
                }");

            var response = await server.RenderResult(builder);
            var expected = @"
                {
                    ""data"": {
                        ""__type"": {
                            ""inputFields"": [
                                { ""name"": ""activeField"", ""isDeprecated"": false },
                                { ""name"": ""count"", ""isDeprecated"": false }
                            ]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public async Task Args_WithIncludeDeprecatedFalse_ExcludesDeprecatedArgs()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder
                .AddGraphController<DeprecatedArgumentsController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                {
                    __type(name: ""Query"") {
                        fields {
                            name
                            args(includeDeprecated: false) {
                                name
                                isDeprecated
                            }
                        }
                    }
                }");

            var response = await server.RenderResult(builder);
            var expected = @"
                {
                    ""data"": {
                        ""__type"": {
                            ""fields"": [
                                {
                                    ""name"": ""getItem"",
                                    ""args"": [
                                        { ""name"": ""id"", ""isDeprecated"": false }
                                    ]
                                },
                                {
                                    ""name"": ""search"",
                                    ""args"": [
                                        { ""name"": ""query"", ""isDeprecated"": false },
                                        { ""name"": ""limit"", ""isDeprecated"": false }
                                    ]
                                }
                            ]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public async Task Args_DeprecatedArg_ExposesDeprecationReason()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder
                .AddGraphController<DeprecatedArgumentsController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                {
                    __type(name: ""Query"") {
                        fields(includeDeprecated: true) {
                            name
                            args(includeDeprecated: true) {
                                name
                                isDeprecated
                                deprecationReason
                            }
                        }
                    }
                }");

            var response = await server.RenderResult(builder);
            var expected = @"
                {
                    ""data"": {
                        ""__type"": {
                            ""fields"": [
                                {
                                    ""name"": ""getItem"",
                                    ""args"": [
                                        { ""name"": ""id"", ""isDeprecated"": false, ""deprecationReason"": null }
                                    ]
                                },
                                {
                                    ""name"": ""search"",
                                    ""args"": [
                                        { ""name"": ""query"", ""isDeprecated"": false, ""deprecationReason"": null },
                                        { ""name"": ""legacySearch"", ""isDeprecated"": true, ""deprecationReason"": ""Use 'query' parameter instead"" },
                                        { ""name"": ""limit"", ""isDeprecated"": false, ""deprecationReason"": null }
                                    ]
                                }
                            ]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public async Task Args_DefaultBehavior_ExcludesDeprecatedArgs()
        {
            var serverBuilder = new TestServerBuilder();
            var server = serverBuilder
                .AddGraphController<DeprecatedArgumentsController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                {
                    __type(name: ""Query"") {
                        fields {
                            name
                            args {
                                name
                                isDeprecated
                            }
                        }
                    }
                }");

            var response = await server.RenderResult(builder);
            var expected = @"
                {
                    ""data"": {
                        ""__type"": {
                            ""fields"": [
                                {
                                    ""name"": ""getItem"",
                                    ""args"": [
                                        { ""name"": ""id"", ""isDeprecated"": false }
                                    ]
                                },
                                {
                                    ""name"": ""search"",
                                    ""args"": [
                                        { ""name"": ""query"", ""isDeprecated"": false },
                                        { ""name"": ""limit"", ""isDeprecated"": false }
                                    ]
                                }
                            ]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public async Task LateBoundDeprecatedInputField_ReturnsTrueDeprecationFlag()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddGraphController<DeprecatedInputFieldsController>();
            var server = serverBuilder.AddGraphQL(o =>
                {
                    o.ApplyDirective("deprecated")
                        .WithArguments("Applied via late binding")
                        .ToItems(schemaItem =>
                            schemaItem != null
                            && schemaItem is IInputGraphField igf
                            && igf.Name == "count");
                })
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                {
                    __type(name: ""DeprecatedFieldsInput"") {
                        inputFields(includeDeprecated: true) {
                            name
                            isDeprecated
                            deprecationReason
                        }
                    }
                }");

            var response = await server.RenderResult(builder);
            var expected = @"
                {
                    ""data"": {
                        ""__type"": {
                            ""inputFields"": [
                                { ""name"": ""activeField"", ""isDeprecated"": false, ""deprecationReason"": null },
                                { ""name"": ""count"", ""isDeprecated"": true, ""deprecationReason"": ""Applied via late binding"" },
                                { ""name"": ""legacyField"", ""isDeprecated"": true, ""deprecationReason"": ""Use activeField instead"" }
                            ]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }

        [Test]
        public async Task LateBoundDeprecatedArgument_ReturnsTrueDeprecationFlag()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddGraphController<DeprecatedArgumentsController>();
            var server = serverBuilder.AddGraphQL(o =>
                {
                    o.ApplyDirective("deprecated")
                        .WithArguments("Applied via late binding")
                        .ToItems(schemaItem =>
                            schemaItem is IGraphArgument { Name: "limit" });
                })
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                {
                    __type(name: ""Query"") {
                        fields {
                            name
                            args(includeDeprecated: true) {
                                name
                                isDeprecated
                                deprecationReason
                            }
                        }
                    }
                }");

            var response = await server.RenderResult(builder);
            var expected = @"
                {
                    ""data"": {
                        ""__type"": {
                            ""fields"": [
                                {
                                    ""name"": ""getItem"",
                                    ""args"": [
                                        { ""name"": ""id"", ""isDeprecated"": false, ""deprecationReason"": null }
                                    ]
                                },
                                {
                                    ""name"": ""search"",
                                    ""args"": [
                                        { ""name"": ""query"", ""isDeprecated"": false, ""deprecationReason"": null },
                                        { ""name"": ""legacySearch"", ""isDeprecated"": true, ""deprecationReason"": ""Use 'query' parameter instead"" },
                                        { ""name"": ""limit"", ""isDeprecated"": true, ""deprecationReason"": ""Applied via late binding"" }
                                    ]
                                }
                            ]
                        }
                    }
                }";

            CommonAssertions.AreEqualJsonStrings(expected, response);
        }
    }
}