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
    using GraphQL.AspNet.Security;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;
    using GraphQL.AspNet.Tests.Security.SecurtyGroupData;

    [TestFixture]
    public class PerRequestAuthorizationTests
    {
        [Test]
        public async Task NoCredentials_ResolvedUnsecuredFields()
        {
            DateTime now = DateTime.UtcNow;
            var serverBuilder = new TestServerBuilder()
                .AddGraphController<SecuredController>()
            .AddGraphQL(options =>
            {
                options.AuthorizationOptions.Method = AuthorizationMethod.PerRequest;
                options.ResponseOptions.TimeStampLocalizer = (x) => now;
            });

            // should produce an error message for each secured field that failed and return no data node
            // ensure that all failed fields are aggregated into the response when doing a "per request" mode
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
                        policySecured {
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
                },
{
                  ""message"": ""Access Denied to field [query]/policySecured"",
                  ""locations"": [
                    {
                      ""line"": 11,
                      ""column"": 25
                    }
                  ],
                  ""path"": [
                    ""policySecured""
                  ],
                  ""extensions"": {
                    ""code"": ""ACCESS_DENIED"",
                    ""timestamp"": ""[TIME_STAMP]"",
                    ""severity"": ""CRITICAL""
                  }
                }
              ]
            }";
            expectedResults = expectedResults.Replace("[TIME_STAMP]", now.ToRfc3339String());

            CommonAssertions.AreEqualJsonStrings(expectedResults, result);
        }

        [Test]
        public async Task WithAuthorizedUser_AllFieldsResolve()
        {
            var serverBuilder = new TestServerBuilder()
                .AddGraphController<SecuredController>()
                .AddGraphQL(options =>
                {
                    options.AuthorizationOptions.Method = AuthorizationMethod.PerRequest;
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
        public async Task WhenNotAuthorized_NoFieldsAreInvoked()
        {
            PerRequestCounterController.NumberOfInvocations = 0;

            var serverBuilder = new TestServerBuilder()
                .AddGraphController<PerRequestCounterController>()
                .AddGraphQL(options =>
                {
                    options.AuthorizationOptions.Method = AuthorizationMethod.PerRequest;
                });

            // should produce 1 error (access to "secured")
            // should produce no data
            // should not have incremented the static property on the controller
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
            Assert.IsNull(result.Data);
            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual(0, PerRequestCounterController.NumberOfInvocations);
            PerRequestCounterController.NumberOfInvocations = 0;
        }
    }
}