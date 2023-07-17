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
    using System.Linq;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.TestData.RuntimeTypeExtensionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using Microsoft.AspNetCore.Hosting.Server;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class RuntimeTypeExtensionTests
    {
        [Test]
        public async Task TypeExtension_OfObject_ResolvesDuringExecution()
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddController<RuntimeFieldTypeExtensionTestsController>();

                    o.MapField<TwoPropertyObject>("Property3", (TwoPropertyObject source) =>
                    {
                        return $"{source.Property1}-{source.Property2}";
                    });
                })
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { retrieveObject { property1 property2 property3 } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""retrieveObject"": {
                            ""property1"" : ""Prop1"",
                            ""property2"" : 101,
                            ""property3"" : ""Prop1-101""
                        }
                    }
                }",
                result);
        }

        [Test]
        public void TypeExtension_OfObject_WithMetaNameField_ThrowsDeclarationException()
        {
            var builder = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddController<RuntimeFieldTypeExtensionTestsController>();

                    o.MapField<TwoPropertyObject>("[Action]", (TwoPropertyObject source) =>
                    {
                        return $"{source.Property1}-{source.Property2}";
                    });
                });

            var ex = Assert.Throws<GraphTypeDeclarationException>(() =>
            {
                builder.Build();
            });

            // ensure the field name is in the message
            Assert.IsTrue(ex.Message.Contains("[Action]"));
        }

        [Test]
        public async Task TypeExtension_NoSourceParameterDeclared_ResolvesCorrectly()
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddController<RuntimeFieldTypeExtensionTestsController>();

                    o.MapField<TwoPropertyObject>("property5", (int argument) =>
                    {
                        return argument;
                    });
                }).Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { retrieveObject { property1 property2 property5(argument: 37) } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""retrieveObject"": {
                            ""property1"" : ""Prop1"",
                            ""property2"" : 101,
                            ""property5"" : 37
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task TypeExtension_OfStruct_ResolvesDuringExecution()
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddController<RuntimeFieldTypeExtensionTestsController>();

                    o.MapField<TwoPropertyStruct>("Property3", (TwoPropertyStruct source) =>
                    {
                        return $"{source.Property1}-{source.Property2}";
                    });
                })
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { retrieveStruct { property1 property2 property3 } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""retrieveStruct"": {
                            ""property1"" : ""Prop1"",
                            ""property2"" : 101,
                            ""property3"" : ""Prop1-101""
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task BatchExtension_OfObject_ResolvesDuringExecution()
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddController<RuntimeFieldTypeExtensionTestsController>();

                    o.MapField<TwoPropertyObject>("Property4")
                        .WithBatchProcessing()
                        .AddResolver((IEnumerable<TwoPropertyObject> source) =>
                        {
                            // leaf property dictionary
                            var dic = new Dictionary<TwoPropertyObject, string>();
                            foreach (var item in source)
                            {
                                dic.Add(item, $"{item.Property1}-{item.Property2}-Batched");
                            }

                            return dic;
                        });
                })
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { retrieveObjects { property1 property2 property4 } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""retrieveObjects"": [{
                            ""property1"" : ""Prop1A"",
                            ""property2"" : 101,
                            ""property4"" : ""Prop1A-101-Batched""
                        },{
                            ""property1"" : ""Prop1B"",
                            ""property2"" : 102,
                            ""property4"" : ""Prop1B-102-Batched""
                        },{
                            ""property1"" : ""Prop1C"",
                            ""property2"" : 103,
                            ""property4"" : ""Prop1C-103-Batched""
                        },]
                    }
                }",
                result);
        }

        [Test]
        public async Task BatchExtension_OfStructs_ResolvesDuringExecution()
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddController<RuntimeFieldTypeExtensionTestsController>();

                    o.MapField<TwoPropertyStruct>("Property4")
                        .WithBatchProcessing()
                        .AddResolver((IEnumerable<TwoPropertyStruct> source) =>
                        {
                            var dic = new Dictionary<TwoPropertyStruct, string>();
                            foreach (var item in source)
                            {
                                dic.Add(item, $"{item.Property1}-{item.Property2}-Batched");
                            }

                            return dic;
                        });
                })
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { retrieveStructs { property1 property2 property4 } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""retrieveStructs"": [{
                            ""property1"" : ""Prop1A"",
                            ""property2"" : 101,
                            ""property4"" : ""Prop1A-101-Batched""
                        },{
                            ""property1"" : ""Prop1B"",
                            ""property2"" : 102,
                            ""property4"" : ""Prop1B-102-Batched""
                        },{
                            ""property1"" : ""Prop1C"",
                            ""property2"" : 103,
                            ""property4"" : ""Prop1C-103-Batched""
                        },]
                    }
                }",
                result);
        }

        [Test]
        public async Task BatchExtension_OfObject_ThatUsesStartBatch_ResolvesDuringExecution()
        {
            var server = new TestServerBuilder()
                .AddGraphQL(o =>
                {
                    o.AddController<RuntimeFieldTypeExtensionTestsController>();

                    o.MapField<TwoPropertyObject>("Property5")
                        .WithBatchProcessing()
                        .AddPossibleTypes(typeof(ChildObject))
                        .AddResolver((IEnumerable<TwoPropertyObject> source) =>
                        {
                            // using static batch builder for child objects
                            return GraphActionResult.StartBatch()
                            .FromSource(source, x => x.Property1)
                            .WithResults(
                                source.Select(x => new ChildObject()
                                {
                                    ParentId = x.Property1,
                                    Property1 = $"Child-Prop1-{x.Property1}",
                                }),
                                y => y.ParentId)
                            .Complete();
                        });
                })
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { retrieveObjects { property1 property2 property5 { property1 } } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""retrieveObjects"": [{
                            ""property1"" : ""Prop1A"",
                            ""property2"" : 101,
                            ""property5"" : {
                                ""property1"": ""Child-Prop1-Prop1A""
                            }
                        },{
                            ""property1"" : ""Prop1B"",
                            ""property2"" : 102,
                            ""property5"" : {
                                ""property1"": ""Child-Prop1-Prop1B""
                            }
                        },{
                            ""property1"" : ""Prop1C"",
                            ""property2"" : 103,
                            ""property5"" : {
                                ""property1"": ""Child-Prop1-Prop1C""
                            }
                        },]
                    }
                }",
                result);
        }

        [Test]
        public async Task Runtime_TypeExtension_WithSecurityParams_AndAllowedUser_ResolvesCorrectly()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.AddController<RuntimeFieldTypeExtensionTestsController>();

                o.MapField<TwoPropertyStruct>("Property3", (TwoPropertyStruct source) =>
                {
                    return $"{source.Property1}-{source.Property2}";
                })
                .RequireAuthorization("policy1");
            });

            serverBuilder.Authorization.AddClaimPolicy("policy1", "policy1Claim", "policy1Value");
            serverBuilder.UserContext
                .Authenticate()
                .AddUserClaim("policy1Claim", "policy1Value");

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { retrieveStruct  { property1 property3  } }");

            var result = await server.RenderResult(builder);
            CommonAssertions.AreEqualJsonStrings(
                @"{
                    ""data"": {
                        ""retrieveStruct"" : {
                            ""property1"": ""Prop1"",
                            ""property3"" : ""Prop1-101""
                        }
                    }
                }",
                result);
        }

        [Test]
        public async Task Runtime_ExecutionDirective_WithSecurityParams_AndUnAuthenticatedUser_RendersAccessDenied()
        {
            var serverBuilder = new TestServerBuilder();

            serverBuilder.AddGraphQL(o =>
            {
                o.AddController<RuntimeFieldTypeExtensionTestsController>();

                o.MapField<TwoPropertyStruct>("Property3", (TwoPropertyStruct source) =>
                {
                    return $"{source.Property1}-{source.Property2}";
                })
                .RequireAuthorization("policy1");
            });

            // user not authenticated
            serverBuilder.Authorization.AddClaimPolicy("policy1", "policy1Claim", "policy1Value");

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { retrieveStruct  { property1 property3  } }");

            var result = await server.ExecuteQuery(builder);
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
                o.AddController<RuntimeFieldTypeExtensionTestsController>();

                o.MapField<TwoPropertyStruct>("Property3", (TwoPropertyStruct source) =>
                {
                    return $"{source.Property1}-{source.Property2}";
                })
                .RequireAuthorization("policy1");
            });

            // incorrect claim
            serverBuilder.Authorization.AddClaimPolicy("policy1", "policy1Claim", "policy1Value");
            serverBuilder.UserContext
                .Authenticate()
                .AddUserClaim("policy1Claim", "policy2Value");

            var server = serverBuilder.Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"query { retrieveStruct  { property1 property3  } }");

            var result = await server.ExecuteQuery(builder);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages[0].Severity);
            Assert.AreEqual(Constants.ErrorCodes.ACCESS_DENIED, result.Messages[0].Code);
        }
    }
}