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
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.TestData.RuntimeFieldTest;
    using GraphQL.AspNet.Tests.Execution.TestData.RuntimeFieldTestData;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class RuntimeFieldTests
    {
        [Test]
        public async Task BasicMappedQuery_ExecutesMethod()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapQuery("/field1/field2", (int a, int b) =>
                    {
                        return a + b;
                    });
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 38
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedQuery_AddingResolverAfter_ExecutesMethod()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapQuery("/field1/field2")
                        .AddResolver((int a, int b) =>
                        {
                            return a + b;
                        });
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 38
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task MappedQuery_ViaGroup_ExecutesMethod()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    var group = o.MapQueryGroup("/field1");
                    group.MapField("/field2", (int a, int b) =>
                    {
                        return a + b;
                    });
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 38
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedMutation_ExecutesMethod()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapMutation("/field1/field2", (int a, int b) =>
                    {
                        return a + b;
                    });
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"mutation { field1 { field2(a: 4, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 37
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedMutation_AddingResolverAfter_ExecutesMethod()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapMutation("/field1/field2")
                    .AddResolver((int a, int b) =>
                    {
                        return a + b;
                    });
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"mutation { field1 { field2(a: 4, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 37
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task StaticMethodMappedDelegate_ThrowsValidationException()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapQuery("/field1/field2", SampleDelegatesForMinimalApi.StaticMethod);
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 4, b: 37 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 41
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task InstanceMethodMappedDelegate_ExecutesMethod()
        {
            var data = new SampleDelegatesForMinimalApi();
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapQuery("/field1/field2", data.InstanceMethod);
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 4, b: 37 } } }");

            // supply no values, allowing the defaults to take overand returning the single
            // requested "Property1" with the default string defined on the method.
            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 41
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedQuery_ReturningActionResult_ResolvesCorrectly()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.MapQuery("/field1/field2")
                        .AddResolver<int>((int a, int b) =>
                        {
                            return GraphActionResult.Ok(a + b);
                        });
                });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 38
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedQuery_WithExplicitlyDeclaredInjectedService_ReturningValueResult_ResolvesCorrectly()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddTransient<IInjectedService, InjectedService>();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("/field1/field2")
                    .AddResolver((int a, int b, [FromServices] IInjectedService injectedService) =>
                    {
                        // injected srvice returns 23
                        return a + b + injectedService.FetchValue();
                    });
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 6, b: 10 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 39
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedQuery_WithExplicitlyDeclaredInjectedService_ReturningActionResult_ResolvesCorrectly()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddTransient<IInjectedService, InjectedService>();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("/field1/field2")
                    .AddResolver<int>((int a, int b, [FromServices] IInjectedService injectedService) =>
                    {
                        // injected srvice returns 23
                        return GraphActionResult.Ok(a + b + injectedService.FetchValue());
                    });
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 61
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedQuery_WithImplicitlyDeclaredInjectedService_ReturningValueResult_ResolvesCorrectly()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddTransient<IInjectedService, InjectedService>();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("/field1/field2")
                    .AddResolver((int a, int b, IInjectedService injectedService) =>
                    {
                        // injected srvice returns 23
                        return a + b + injectedService.FetchValue();
                    });
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 6, b: 10 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 39
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BasicMappedQuery_WithImplicitlyDeclaredInjectedService_ReturningActionResult_ResolvesCorrectly()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddTransient<IInjectedService, InjectedService>();

            serverBuilder.AddGraphQL(o =>
            {
                o.DeclarationOptions.ArgumentBindingRule = SchemaArgumentBindingRules.ParametersPreferQueryResolution;

                o.MapQuery("/field1/field2")
                    .AddResolver<int>((int a, int b, IInjectedService injectedService) =>
                    {
                        // injected srvice returns 23
                        return GraphActionResult.Ok(a + b + injectedService.FetchValue());
                    });
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field1 { field2(a: 5, b: 33 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field1"": {
                            ""field2"" : 61
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task ServiceInjectedOnControllerAction_ResolvesCorrectly()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddTransient<IInjectedService, InjectedService>();
            serverBuilder.AddController<ControllerWithInjectedService>();

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();

            // injected service will supply 23
            builder.AddQueryText(@"query { add(arg1: 5) }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""add"" : 28
                    }
                }",
                result);
        }

        [Test]
        public async Task Runtime_StandardField_WithSecurityParams_AndAllowedUser_ResolvesCorrectly()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("secureField", () => 3)
                 .RequireAuthorization("policy1");
            });

            serverBuilder.Authorization.AddClaimPolicy("policy1", "policy1Claim", "policy1Value");
            serverBuilder.UserContext
                .Authenticate()
                .AddUserClaim("policy1Claim", "policy1Value");

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { secureField }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""secureField"": 3
                    }
                }",
                result);
        }

        [Test]
        public async Task Runtime_StandardField_WithSecurityParams_AndUnAuthenticatedUser_RendersAccessDenied()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("secureField", () => 3)
                 .RequireAuthorization("policy1");
            });

            // no user authentication added
            serverBuilder.Authorization.AddClaimPolicy("policy1", "policy1Claim", "policy1Value");

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { secureField }");

            var result = await server.ExecuteQuery(builder);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages[0].Severity);
            Assert.AreEqual(Constants.ErrorCodes.ACCESS_DENIED, result.Messages[0].Code);
        }

        [Test]
        public async Task Runtime_StandardField_WithSecurityParams_AndUnAuthorizedUser_RendersAccessDenied()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("secureField", () => 3)
                 .RequireAuthorization("policy1");
            });

            // wrong claim value on user
            serverBuilder.Authorization.AddClaimPolicy("policy1", "policy1Claim", "policy1Value");
            serverBuilder.UserContext
                .Authenticate()
                .AddUserClaim("policy1Claim", "policy2Value");

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { secureField }");

            var result = await server.ExecuteQuery(builder);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages[0].Severity);
            Assert.AreEqual(Constants.ErrorCodes.ACCESS_DENIED, result.Messages[0].Code);
        }

        [Test]
        public async Task Runtime_StandardField_ReturnsNullableT_RendersValue()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("field", () => new int?(3));
            });

            var server = serverBuilder.Build();

            var field = server.Schema.Operations[GraphOperationType.Query].Fields.Single(x => x.Name == "field");
            Assert.AreEqual(typeof(int), field.ObjectType);
            Assert.AreEqual("Int", field.TypeExpression.ToString()); // no !, not required

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                  ""data"": {
                    ""field"": 3
                  }
                }",
                result);
        }

        [Test]
        public async Task Runtime_StandardField_ReturnsNullableT_RendersNull()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("field", () =>
                {
                    int? value = null;
                    return value;
                });
            });

            var server = serverBuilder.Build();

            var field = server.Schema.Operations[GraphOperationType.Query].Fields.Single(x => x.Name == "field");
            Assert.AreEqual(typeof(int), field.ObjectType);
            Assert.AreEqual("Int", field.TypeExpression.ToString()); // no !, not required

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                  ""data"": {
                    ""field"": null
                  }
                }",
                result);
        }

        [Test]
        public async Task Runtime_StandardField_ReturnsNullableT_ThroughActionResult()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("field", () =>
                {
                    return GraphActionResult.Ok(3);
                })
                .AddPossibleTypes(typeof(int?));
            });

            var server = serverBuilder.Build();

            var field = server.Schema.Operations[GraphOperationType.Query].Fields.Single(x => x.Name == "field");
            Assert.AreEqual(typeof(int), field.ObjectType);
            Assert.AreEqual("Int", field.TypeExpression.ToString()); // no !, not required

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                  ""data"": {
                    ""field"": 3
                  }
                }",
                result);
        }

        [Test]
        public async Task Runtime_StandardField_FieldResolutionContext_IsInjected_WhenRequested()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("field", (FieldResolutionContext context) =>
                {
                    if (context != null && context.Request.Field != null)
                        return 1;

                    return 0;
                });
            });

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                  ""data"": {
                    ""field"": 1
                  }
                }",
                result);
        }
    }
}