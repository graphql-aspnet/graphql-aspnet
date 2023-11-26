// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers
{
    using System.Reflection;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Controllers.ActionResults;
    using GraphQL.AspNet.Controllers.InputModel;
    using GraphQL.AspNet.Tests.Common.CommonHelpers;
    using GraphQL.AspNet.Tests.Controllers.ControllerTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class GraphControllerTests
    {
        [Test]
        public async Task MethodInvocation_EnsureInternalPropertiesAreSet()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateFieldContextBuilder<InvokableController>(
                nameof(InvokableController.AsyncActionMethod));
            fieldContextBuilder.AddInputArgument("arg1", "random string");

            var resolutionContext = fieldContextBuilder.CreateResolutionContext();

            var controller = new InvokableController();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.ResolverMetaData, resolutionContext);

            Assert.IsNotNull(result);
            Assert.IsTrue(result is OperationCompleteGraphActionResult);

            Assert.AreEqual(3, controller.CapturedItems.Count);

            Assert.AreEqual(server.ServiceProvider, controller.CapturedItems["RequestServices"]);
            Assert.AreEqual(resolutionContext.Request, controller.CapturedItems["Request"]);
            var modelState = controller.CapturedItems["ModelState"] as InputModelStateDictionary;

            Assert.IsTrue(modelState.IsValid);
            Assert.IsTrue(modelState.ContainsKey("arg1"));
        }

        [Test]
        public async Task MethodInvocation_SyncMethodReturnsObjectNotTask()
        {
            var tester = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();

            var fieldContextBuilder = tester.CreateFieldContextBuilder<InvokableController>(
                nameof(InvokableController.SyncronousActionMethod));
            fieldContextBuilder.AddInputArgument("arg1", "random string");

            var controller = new InvokableController();
            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.ResolverMetaData, resolutionContext);

            Assert.IsNotNull(result);
            Assert.IsTrue(result is OperationCompleteGraphActionResult);
        }

        [Test]
        public async Task MethodInvocation_UnawaitableAsyncMethodFlag_ResultsInInternalError()
        {
            var tester = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();
            var fieldContextBuilder = tester.CreateFieldContextBuilder<InvokableController>(
                nameof(InvokableController.SyncronousActionMethod));
            fieldContextBuilder.AddInputArgument("arg1", "random string");

            fieldContextBuilder.ResolverMetaData.IsAsyncField.Returns(true);

            var controller = new InvokableController();
            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.ResolverMetaData, resolutionContext);

            // ensure a server error reslt is generated
            Assert.IsNotNull(result);
            Assert.IsTrue(result is InternalServerErrorGraphActionResult);
        }

        [Test]
        public async Task MethodInvocation_MissingMethodInfo_ReturnsInternalServerError()
        {
            var tester = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();
            var fieldContextBuilder = tester.CreateFieldContextBuilder<InvokableController>(
                nameof(InvokableController.SyncronousActionMethod));
            fieldContextBuilder.AddInputArgument("arg1", "random string");
            fieldContextBuilder.ResolverMetaData.Method.Returns(null as MethodInfo);

            var controller = new InvokableController();
            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var result = await controller.InvokeActionAsync(fieldContextBuilder.ResolverMetaData, resolutionContext);

            // ensure a server error reslt is generated
            Assert.IsNotNull(result);
            Assert.IsTrue(result is InternalServerErrorGraphActionResult);
        }

        [Test]
        public void MethodInvocation_UserCodeExceptionIsAllowedToThrow()
        {
            var tester = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddGraphController<InvokableController>()
                .Build();
            var fieldContextBuilder = tester.CreateFieldContextBuilder<InvokableController>(
                nameof(InvokableController.AsyncActionMethodToCauseException));

            fieldContextBuilder.AddInputArgument("arg1", "random string");

            var controller = new InvokableController();
            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            Assert.ThrowsAsync<UserThrownException>(async () => await controller.InvokeActionAsync(fieldContextBuilder.ResolverMetaData, resolutionContext));
        }

        [Test]
        public async Task ErrorResult()
        {
            var server = new TestServerBuilder(TestOptions.UseCodeDeclaredNames)
                .AddType<InvokableController>()
                .Build();

            var fieldContextBuilder = server.CreateFieldContextBuilder<InvokableController>(
                nameof(InvokableController.ErrorResult));

            var controller = new InvokableController();
            var resolutionContext = fieldContextBuilder.CreateResolutionContext();
            var result = await controller.InvokeActionAsync(
                fieldContextBuilder.ResolverMetaData,
                resolutionContext) as GraphFieldErrorActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("an error happened", result.Message);
            Assert.AreEqual("12345", result.Code);
            Assert.IsNotNull(result.Exception);
            Assert.AreEqual("exception text", result.Exception.Message);
        }

        [Test]
        public async Task ErrorResultBySeverity()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<InvokableController>();
            builder.AddGraphQL(
                x =>
                {
                    x.ResponseOptions.TimeStampLocalizer =
                        (x) => new System.DateTime(1900, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                });

            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"query {
                        invoke {
                            errorResultBySeverity
                        }
                    }");

            var expectedJson = @"
            {
              ""errors"": [
                {
                  ""message"": ""my message"",
                  ""extensions"": {
                    ""code"": ""my code"",
                    ""timestamp"": ""1900-01-01T00:00:00.000+00:00"",
                    ""severity"": ""WARNING""
                  }
                }
              ]
            }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task InternalServerErrorResult()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<InvokableController>();
            builder.AddGraphQL(
                x =>
                {
                    x.ResponseOptions.TimeStampLocalizer =
                        (x) => new System.DateTime(1900, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                });

            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"query {
                        invoke {
                            internalServerErrorResult
                        }
                    }");

            var expectedJson = @"
                {
                  ""errors"": [
                    {
                      ""message"": ""Server error"",
                      ""locations"": [
                        {
                          ""line"": 3,
                          ""column"": 29
                        }
                      ],
                      ""path"": [
                        ""invoke"",
                        ""internalServerErrorResult""
                      ],
                      ""extensions"": {
                        ""code"": ""INTERNAL_SERVER_ERROR"",
                        ""timestamp"": ""1900-01-01T00:00:00.000+00:00"",
                        ""severity"": ""CRITICAL""
                      }
                    }
                  ]
                }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task NotFoundResult()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<InvokableController>();
            builder.AddGraphQL(
                x =>
                {
                    x.ResponseOptions.TimeStampLocalizer =
                        (x) => new System.DateTime(1900, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                });

            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"query {
                        invoke {
                            notFoundResult
                        }
                    }");

            var expectedJson = @"
                {
                    ""errors"": [
                    {
                        ""message"": ""thing wasnt found"",
                        ""locations"": [
                        {
                            ""line"": 3,
                            ""column"": 29
                        }
                        ],
                        ""path"": [
                        ""invoke"",
                        ""notFoundResult""
                        ],
                        ""extensions"": {
                        ""code"": ""INVALID_PATH"",
                        ""timestamp"": ""1900-01-01T00:00:00.000+00:00"",
                        ""severity"": ""CRITICAL""
                        }
                    }
                    ]
                }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task UnAuthorizedResult()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<InvokableController>();
            builder.AddGraphQL(
                x =>
                {
                    x.ResponseOptions.TimeStampLocalizer =
                        (x) => new System.DateTime(1900, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                });

            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"query {
                        invoke {
                            unauthorizedResult
                        }
                    }");

            var expectedJson = @"
                {
                  ""errors"": [
                    {
                      ""message"": ""nope not authorized"",
                      ""locations"": [
                        {
                          ""line"": 3,
                          ""column"": 29
                        }
                      ],
                      ""path"": [
                        ""invoke"",
                        ""unauthorizedResult""
                      ],
                      ""extensions"": {
                        ""code"": ""ACCESS_DENIED"",
                        ""timestamp"": ""1900-01-01T00:00:00.000+00:00"",
                        ""severity"": ""CRITICAL""
                      }
                    }
                  ]
                }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task OkNoObjectResult()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<InvokableController>();

            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"query {
                        invoke {
                            okNoObjectResult
                        }
                    }");

            var expectedJson = @"
               {
                    ""data"": {
                    ""invoke"": {
                        ""okNoObjectResult"": null
                    }
                    }
                }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task BadRequestMessageResult()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<InvokableController>();
            builder.AddGraphQL(
                x =>
                {
                    x.ResponseOptions.TimeStampLocalizer =
                        (x) => new System.DateTime(1900, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                });

            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"query {
                        invoke {
                            badRequestMessageResult
                        }
                    }");

            var expectedJson = @"
               {
                  ""errors"": [
                    {
                      ""message"": ""it was bad"",
                      ""locations"": [
                        {
                          ""line"": 3,
                          ""column"": 29
                        }
                      ],
                      ""path"": [
                        ""invoke"",
                        ""badRequestMessageResult""
                      ],
                      ""extensions"": {
                        ""code"": ""BAD_REQUEST"",
                        ""timestamp"": ""1900-01-01T00:00:00.000+00:00"",
                        ""severity"": ""CRITICAL""
                      }
                    }
                  ],
                  ""data"": {
                    ""invoke"": {
                      ""badRequestMessageResult"": null
                    }
                  }
                }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }

        [Test]
        public async Task BadRequestObjectResult()
        {
            var builder = new TestServerBuilder();
            builder.AddGraphController<InvokableController>();
            builder.AddGraphQL(
                x =>
                {
                    x.ResponseOptions.TimeStampLocalizer =
                        (x) => new System.DateTime(1900, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                });

            var server = builder.Build();

            var queryContext = server.CreateQueryContextBuilder();
            queryContext.AddQueryText(
                @"query {
                        invoke {
                            badRequestModelResult(model: {name: ""A Very Long Name""})
                        }
                    }");

            var expectedJson = @"
               {
                  ""errors"": [
                    {
                      ""message"": ""The field Name must be a string with a maximum length of 5."",
                      ""locations"": [
                        {
                          ""line"": 3,
                          ""column"": 29
                        }
                      ],
                      ""path"": [
                        ""invoke"",
                        ""badRequestModelResult""
                      ],
                      ""extensions"": {
                        ""code"": ""MODEL_VALIDATION_ERROR"",
                        ""timestamp"": ""1900-01-01T00:00:00.000+00:00"",
                        ""severity"": ""CRITICAL""
                      }
                    },
                    {
                      ""message"": ""Invalid model data."",
                      ""locations"": [
                        {
                          ""line"": 3,
                          ""column"": 29
                        }
                      ],
                      ""path"": [
                        ""invoke"",
                        ""badRequestModelResult""
                      ],
                      ""extensions"": {
                        ""code"": ""BAD_REQUEST"",
                        ""timestamp"": ""1900-01-01T00:00:00.000+00:00"",
                        ""severity"": ""CRITICAL""
                      }
                    }
                  ],
                  ""data"": {
                    ""invoke"": {
                      ""badRequestModelResult"": null
                    }
                  }
                }";

            var result = await server.RenderResult(queryContext);
            CommonAssertions.AreEqualJsonStrings(expectedJson, result);
        }
    }
}