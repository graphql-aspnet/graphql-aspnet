// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Security
{
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.CommonHelpers;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using GraphQL.AspNet.Tests.Security.SecurtyGroupData;
    using NUnit.Framework;

    [TestFixture]
    public class PerFieldAuthorizationTests
    {
        [Test]
        public async Task NoCredentials_ResolvedUnsecuredFields()
        {
            DateTime now = DateTime.UtcNow;
            var serverBuilder = new TestServerBuilder()
                .AddGraphController<SecuredController>()
            .AddGraphQL(options =>
            {
                options.AuthorizationOptions.Method = AuthorizationMethod.PerField;
                options.ResponseOptions.TimeStampLocalizer = (x) => now;
            });

            // should produce an error message and return null as the field value for "secured"
            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                    query {
                        unsecured {
                            property1
                            property2
                        }
                        secured {
                           property1
                           property2
                        }
                    }");

            var result = await server.RenderResult(builder);
            var expectedResults = @"
            {
              ""errors"": [
                {
                  ""message"": ""Access Denied to field [query]/secured"",
                  ""locations"": [
                    {
                      ""line"": 7,
                      ""column"": 25
                    }
                  ],
                  ""path"": [
                    ""secured""
                  ],
                  ""extensions"": {
                    ""code"": ""ACCESS_DENIED"",
                    ""timestamp"": ""[TIME_STAMP]"",
                    ""severity"": ""CRITICAL""
                  }
                }
              ],
              ""data"": {
                ""unsecured"": {
                  ""property1"": ""unsecure object"",
                  ""property2"": 1
                },
                ""secured"" : null
              }
            }";
            expectedResults = expectedResults.Replace("[TIME_STAMP]", now.ToRfc3339String());

            CommonAssertions.AreEqualJsonStrings(expectedResults, result);
        }

        [Test]
        public async Task WithAuthorizedUser_ButWithoutPolicyResolvesAllFields()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphController<SecuredController>()
                .AddGraphQL(options =>
                {
                    options.AuthorizationOptions.Method = AuthorizationMethod.PerField;
                });

            serverBuilder.User.Authenticate();

            // should produce no errors and render both fields
            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                    query {
                        unsecured {
                            property1
                            property2
                        }
                        secured {
                           property1
                           property2
                        }
                    }");

            var result = await server.RenderResult(builder);
            var expectedResults = @"
            {
              ""data"": {
                ""unsecured"": {
                  ""property1"": ""unsecure object"",
                  ""property2"": 1
                },
                ""secured"": {
                  ""property1"": ""secured object"",
                  ""property2"": 5
                }
              }
            }";

            CommonAssertions.AreEqualJsonStrings(expectedResults, result);
        }

        [Test]
        public async Task WhenNotAuthorized_TheUnsecuredFieldIsStillCalled()
        {
            PerFieldCounterController.NumberOfInvocations = 0;

            var serverBuilder = new TestServerBuilder()
                .AddGraphController<PerFieldCounterController>()
                .AddGraphQL(options =>
                {
                    options.AuthorizationOptions.Method = AuthorizationMethod.PerField;
                });

            // should produce 1 error (access to "secured")
            // should produce data (the "unsecured" field)
            // should have incremented the static property on the controller 1 time for the unsecured field
            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                    query {
                        unsecured {
                            property1
                            property2
                        }
                        secured {
                           property1
                           property2
                        }
                    }");

            var result = await server.ExecuteQuery(builder);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(1, PerFieldCounterController.NumberOfInvocations);
            PerFieldCounterController.NumberOfInvocations = 0;
        }
    }
}