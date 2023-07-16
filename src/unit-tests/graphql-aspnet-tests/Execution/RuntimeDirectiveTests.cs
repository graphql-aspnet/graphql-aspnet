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
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.TestData.RuntimeDirectiveTestData;
    using GraphQL.AspNet.Tests.Execution.TestData.RuntimeFieldTestData;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class RuntimeDirectiveTests
    {
        private static Dictionary<string, object> _values = new Dictionary<string, object>();

        [Test]
        public void Runtime_TypeSystemDirective_IsInvokedCorrectly()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.AddType<TwoPropertyObject>();
                o.MapDirective("@myObjectDirective")
                    .RestrictLocations(DirectiveLocation.OBJECT)
                    .AddResolver<int>((int a, int b) =>
                    {
                        _values["generalTypeSystemDirective"] = a + b;
                        return GraphActionResult.Ok();
                    });

                o.ApplyDirective("myObjectDirective")
                    .ToItems(x => x.IsObjectGraphType<TwoPropertyObject>())
                    .WithArguments(5, 18);
            });

            var server = serverBuilder.Build();
            Assert.AreEqual(23, _values["generalTypeSystemDirective"]);
        }

        [Test]
        public void Runtime_TypeSystemDirective_InjectedService_IsInvokedCorrectly()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddTransient<IInjectedService, InjectedService>();

            serverBuilder.AddGraphQL(o =>
            {
                o.AddType<TwoPropertyObject>();
                o.MapDirective("@myObjectDirective")
                    .RestrictLocations(DirectiveLocation.OBJECT)
                    .AddResolver<int>((int a, IInjectedService service) =>
                    {
                        _values["injectedService"] = a + service.FetchValue();
                        return GraphActionResult.Ok();
                    });

                o.ApplyDirective("myObjectDirective")
                    .ToItems(x => x.IsObjectGraphType<TwoPropertyObject>())
                    .WithArguments(5);
            });

            var server = serverBuilder.Build();

            // injected services supplies 23
            Assert.AreEqual(28, _values["injectedService"]);
        }

        [Test]
        public void Runtime_TypeSystemDirective_InterfaceAsExplicitSchemaItem_ThrowsException()
        {
            var serverBuilder = new TestServerBuilder();
            serverBuilder.AddTransient<IInjectedService, InjectedService>();

            serverBuilder.AddGraphQL(o =>
            {
                o.AddType<TwoPropertyObject>();
                o.MapDirective("@myObjectDirective")
                    .RestrictLocations(DirectiveLocation.OBJECT)
                    .AddResolver<int>((int a, [FromGraphQL] IInjectedService service) =>
                    {
                        _values["injectedServiceWrong"] = a + service.FetchValue();
                        return GraphActionResult.Ok();
                    });

                o.ApplyDirective("myObjectDirective")
                    .ToItems(x => x.IsObjectGraphType<TwoPropertyObject>())
                    .WithArguments(5);
            });

            Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                var server = serverBuilder.Build();
            });
        }

        [Test]
        public async Task Runtime_ExecutionDirective_IsInvokedCorrectly()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.AddController<SingleFieldController>();
                o.MapDirective("@myFieldDirective")
                    .RestrictLocations(DirectiveLocation.FIELD)
                    .AddResolver<int>(() =>
                    {
                        _values["fieldDirective"] = 11;
                        return GraphActionResult.Ok();
                    });
            });

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field @myFieldDirective }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field"": 99
                    }
                }",
                result);

            Assert.AreEqual(11, _values["fieldDirective"]);
        }

        [Test]
        public async Task Runtime_ExecutionDirective_OnMinimalApiField_IsInvokedCorrectly()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("field", () => 3);

                o.MapDirective("@myFieldDirective")
                    .RestrictLocations(DirectiveLocation.FIELD)
                    .AddResolver<int>(() =>
                    {
                        _values["fieldDirective"] = 11;
                        return GraphActionResult.Ok();
                    });
            });

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field @myFieldDirective }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field"": 3
                    }
                }",
                result);

            Assert.AreEqual(11, _values["fieldDirective"]);
        }

        [Test]
        public async Task Runtime_ExecutionDirective_WithSecurityParams_AndAllowedUser_ResolvesCorrectly()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("field", () => 3);

                o.MapDirective("@secureDirective")
                    .RestrictLocations(DirectiveLocation.FIELD)
                    .RequireAuthorization("policy1")
                    .AddResolver<int>(() =>
                    {
                        _values["secureDirective1"] = 11;
                        return GraphActionResult.Ok();
                    });
            });

            serverBuilder.Authorization.AddClaimPolicy("policy1", "policy1Claim", "policy1Value");
            serverBuilder.UserContext
                .Authenticate()
                .AddUserClaim("policy1Claim", "policy1Value");

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field @secureDirective }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""field"": 3
                    }
                }",
                result);

            Assert.AreEqual(11, _values["secureDirective1"]);
        }

        [Test]
        public async Task Runtime_ExecutionDirective_WithSecurityParams_AndUnAuthenticatedUser_RendersAccessDenied()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("field", () => 3);

                o.MapDirective("@secureDirective")
                    .RestrictLocations(DirectiveLocation.FIELD)
                    .RequireAuthorization("policy1")
                    .AddResolver<int>(() =>
                    {
                        _values["secureDirective2"] = 11;
                        return GraphActionResult.Ok();
                    });
            });

            // no user authentication added
            serverBuilder.Authorization.AddClaimPolicy("policy1", "policy1Claim", "policy1Value");

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field @secureDirective }");

            var result = await server.ExecuteQuery(builder);
            Assert.IsFalse(_values.ContainsKey("secureDirective2"));
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages[0].Severity);
            Assert.AreEqual(Constants.ErrorCodes.ACCESS_DENIED, result.Messages[0].Code);
        }

        [Test]
        public async Task Runtime_ExecutionDirective_WithSecurityParams_AndUnAuthorizedUser_RendersAccessDenied()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.MapQuery("field", () => 3);

                o.MapDirective("@secureDirective")
                    .RestrictLocations(DirectiveLocation.FIELD)
                    .RequireAuthorization("policy1")
                    .AddResolver<int>(() =>
                    {
                        _values["secureDirective3"] = 11;
                        return GraphActionResult.Ok();
                    });
            });

            // wrong policy value
            serverBuilder.Authorization.AddClaimPolicy("policy1", "policy1Claim", "policy1Value");
            serverBuilder.UserContext
                .Authenticate()
                .AddUserClaim("policy1Claim", "policy2Value");

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { field @secureDirective }");

            var result = await server.ExecuteQuery(builder);
            Assert.IsFalse(_values.ContainsKey("secureDirective3"));
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages[0].Severity);
            Assert.AreEqual(Constants.ErrorCodes.ACCESS_DENIED, result.Messages[0].Code);
        }
    }
}