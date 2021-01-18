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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Execution.InterfaceExtensionTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class InterfaceExecutionTests
    {
        [Test]
        public async Task TypeExtension_AppliedToAnInterface_CanBeActedOnByMultipleConcreteObjects()
        {
            var server = new TestServerBuilder()

                .AddGraphType<ConcreteObjectA>()
                .AddGraphType<ConcreteObjectB>()
                .AddGraphType<InterfaceExtensionController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                                {
                                    multiObjects {
                                       firstName
                                       fullName
                                    }
                                }");

            // fullName is an type extension keyed on the interface that both object types inherit it does not exist as a declared
            // member of ObjectA, B or the common interface
            // the interface explicitly DOES NOT declare FirstName or LastName ensuring they must be generated from the
            // concrete types
            var result = await server.RenderResult(builder);

            // output combines declared methods on concrete objects, firstName, and the interface extension, fullName
            var expectedOutput = @"
                        {
                          ""data"": {
                                    ""multiObjects"": [
                                    {
                                        ""firstName"" : ""0A_prop1"",
                                        ""fullName"": ""0A_prop1 0A_prop2""
                                    },
                                    {
                                        ""firstName"" : ""1A_prop1"",
                                        ""fullName"": ""1A_prop1 1A_prop2""
                                    },
                                    {
                                        ""firstName"" : ""0B_prop1"",
                                        ""fullName"": ""0B_prop1 0B_prop2""
                                    },
                                    {
                                        ""firstName"" : ""1B_prop1"",
                                        ""fullName"": ""1B_prop1 1B_prop2""
                                    }
                                    ]
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task FieldOnConcreteTypes_ButNotOnSharedInterface_WhenInterfaceIsReturned_IsDenied()
        {
            var server = new TestServerBuilder()
                .AddGraphType<ConcreteObjectA>()
                .AddGraphType<ConcreteObjectB>()
                .AddGraphType<InterfaceExtensionController>()
                .Build();

            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                                {
                                    multiObjects {
                                       middleName
                                       fullName
                                    }
                                }");

            // MiddleName is declared on ObjectA but NOT on the interface returned by 'multiObjects'
            // since the return type of multiObjects IS the interface it should be denied as a
            // valid field even though the underlying concrete types could process it
            var result = await server.ExecuteQuery(builder);
            Assert.AreEqual(GraphMessageSeverity.Critical, result.Messages.Severity);
        }

        [Test]
        public async Task WhenAnObjectReturnedImplmentsTheInterface_ButIsntInTheSchema_DenyIt()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphType<ConcreteObjectA>()
                .AddGraphType<ConcreteObjectB>()
                .AddGraphType<InterfaceExtensionController>();

            serverBuilder.AddGraphQL(o =>
            {
                o.ResponseOptions.MessageSeverityLevel = GraphMessageSeverity.Critical;
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                                {
                                    multiObjectsWithC {
                                       firstName
                                       fullName
                                    }
                                }");

            // 'multiObjectsWithC' returns 3 objects of type A, B, C and all three implement the interface
            // but ONLY A and B are declared on the schema.
            // Since the schema doesnt know about object C it has no way to deal with the
            // field resolution and results in an error
            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);

            var message = result.Messages[0];
            Assert.AreEqual(Constants.ErrorCodes.EXECUTION_ERROR, message.Code);
            Assert.IsTrue(message.Exception.Message.Contains(nameof(ConcreteObjectC)));
        }

        [Test]
        public async Task FieldOnConcreteTypes_ButNotOnSharedInterface_WhenInterfaceIsReturned_ButTypeConditionedFragmentsAreUsed_IsAllowed()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphType<ConcreteObjectA>()
                .AddGraphType<ConcreteObjectB>()
                .AddGraphType<InterfaceExtensionController>();

            serverBuilder.AddGraphQL(o =>
            {
                o.ResponseOptions.MessageSeverityLevel = GraphMessageSeverity.Critical;
            });

            // this query should include un-navigable routes
            // which may report being unprocessable as an info level warning (its expected)
            // disregard them
            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                                {
                                    multiObjects {
                                       fullName
                                        ... on ConcreteObjectA {
                                            middleName
                                        }
                                        ... on ConcreteObjectB {
                                            title
                                        }
                                    }
                                }");

            // 'MiddleName' is declared on ObjectA and and 'Title' ObjectB but neither are declared on the interface
            // respective objects should still return their data.
            var result = await server.RenderResult(builder);

            // output combines declared methods on concrete objects, firstName, and the interface extension, fullName
            var expectedOutput = @"
                        {
                          ""data"": {
                                    ""multiObjects"": [
                                    {
                                        ""fullName"": ""0A_prop1 0A_prop2"",
                                        ""middleName"" : ""0A_prop3""
                                    },
                                    {
                                        ""fullName"": ""1A_prop1 1A_prop2"",
                                        ""middleName"" : ""1A_prop3""
                                    },
                                    {
                                        ""fullName"": ""0B_prop1 0B_prop2"",
                                        ""title"" : ""0B_prop3""
                                    },
                                    {
                                        ""fullName"": ""1B_prop1 1B_prop2"",
                                        ""title"" : ""1B_prop3""
                                    }
                                    ]
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }
    }
}