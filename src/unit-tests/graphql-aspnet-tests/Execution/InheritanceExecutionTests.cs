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
    using GraphQL.AspNet.Tests.Execution.TestData.InheritanceTestData;
    using GraphQL.AspNet.Tests.Framework;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;
    using NUnit.Framework;

    [TestFixture]
    public class InheritanceExecutionTests
    {
        [Test]
        public async Task FieldOnBaseInterface_IsAvailalbleOnSubInterfaceWhenBothAreIncluded()
        {
            var serverBuilder = new TestServerBuilder()
            .AddType<IBox>()
            .AddType<ISquare>()
            .AddType<Box>()
            .AddGraphController<BoxController>();

            serverBuilder.AddGraphQL(o =>
            {
                o.ResponseOptions.MessageSeverityLevel = GraphMessageSeverity.Critical;
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                                {
                                    retrieveBoxInterface {
                                        width
                                        height
                                        length
                                    }
                                }");

            // oct2021 spec change
            // retrieveBoxInterface returns an IBox. IBox does NOT declare width and hieght
            // but they are declared on ISquare. Since IBox implements ISquare and both are
            // on the schema IBox should be updated to incldue the two missing
            // properties and they should be navigable and produce values
            var result = await server.RenderResult(builder);

            var expectedOutput = @"
                        {
                          ""data"": {
                                    ""retrieveBoxInterface"": {
                                        ""width"": ""width2"",
                                        ""height"" : ""height2"",
                                        ""length"" : ""length2"",
                                    }
                                }
                            }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }

        [Test]
        public async Task FieldOnSuperInterface_IsNOTAvailalbleOnSubInterface_WhenSuperIsNotInclude()
        {
            var serverBuilder = new TestServerBuilder()
            .AddType<IBox>()
            .AddType<Box>()
            .AddGraphController<BoxNoSquareController>();

            serverBuilder.AddGraphQL(o =>
            {
                o.ResponseOptions.MessageSeverityLevel = GraphMessageSeverity.Critical;
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                                {
                                    retrieveBoxInterface {
                                        width
                                        height
                                        length
                                    }
                                }");

            // oct2021 spec change
            // IBox implements ISquare but since ISquare is not included in the graph
            // its fields should not be available to IBox
            // this should result in an invalid document when querying for width and height
            // by box
            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(2, result.Messages.Count);
            Assert.AreEqual("5.3.1", result.Messages[0].MetaData["Rule"].ToString());
            Assert.AreEqual(Constants.ErrorCodes.INVALID_DOCUMENT, result.Messages[0].Code);
            Assert.IsTrue(result.Messages[0].Message.Contains("width"));

            Assert.AreEqual("5.3.1", result.Messages[1].MetaData["Rule"].ToString());
            Assert.AreEqual(Constants.ErrorCodes.INVALID_DOCUMENT, result.Messages[1].Code);
            Assert.IsTrue(result.Messages[1].Message.Contains("length"));
        }

        [Test]
        public async Task TypeExtensionToObject_IsNOTSeenByInterfaces()
        {
            var serverBuilder = new TestServerBuilder()
            .AddType<IBox>()
            .AddType<ISquare>()
            .AddType<Box>()
            .AddGraphController<BoxController>();

            serverBuilder.AddGraphQL(o =>
            {
                o.ResponseOptions.MessageSeverityLevel = GraphMessageSeverity.Critical;
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                                {
                                    retrieveBoxInterface {
                                        width
                                        height
                                        length
                                        area
                                        corners
                                    }
                                }");

            // oct2021 spec change
            // corners is a type extension on Box not on IBox
            // this query should result in an error
            // as the corners field should not be applied to IBox
            // even thtough Box implements it
            var result = await server.ExecuteQuery(builder);

            Assert.AreEqual(1, result.Messages.Count);
            Assert.AreEqual("5.3.1", result.Messages[0].MetaData["Rule"].ToString());
            Assert.AreEqual(Constants.ErrorCodes.INVALID_DOCUMENT, result.Messages[0].Code);
        }

        [Test]
        public async Task TypeExtensionToObject_IsNOTSeenByInterfaces_ButIsAvailableToFragment()
        {
            var serverBuilder = new TestServerBuilder()
                .AddType<IBox>()
                .AddType<ISquare>()
                .AddType<Box>()
                .AddType<Box2>()
                .AddGraphController<BoxController>();

            serverBuilder.AddGraphQL(o =>
            {
                o.ResponseOptions.MessageSeverityLevel = GraphMessageSeverity.Critical;
            });

            var server = serverBuilder.Build();
            var builder = server.CreateQueryContextBuilder();
            builder.AddQueryText(@"
                                {
                                    retrieveDoubleBoxInterface {
                                        width
                                        height
                                        length
                                        area
                                        ... on Box {
                                        corners
                                        }
                                    }
                                }");

            // oct2021 spec change
            // corners is a type extension on Box not on IBox
            // this method returns 2 boxes of tpyes Box and Box2
            // they both implement IBox and IBox is the return type of the method
            // However, corners should only be available to Box NOT Box2
            // because the type extension is type specific, not interface bound
            var result = await server.RenderResult(builder);

            var expectedOutput = @"
                {
                  ""data"": {
                    ""retrieveDoubleBoxInterface"": [
                      {
                        ""width"": ""box1Width"",
                        ""height"": ""box1Height"",
                        ""length"": ""box1Length"",
                        ""area"": ""box1Length|box1Width"",
                        ""corners"": ""box1Length|6""
                      },
                      {
                        ""width"": ""box2Width"",
                        ""height"": ""box2Height"",
                        ""length"": ""box2Lenght"",
                        ""area"": ""box2Lenght|box2Width""
                      }
                    ]
                  }
                }";

            CommonAssertions.AreEqualJsonStrings(expectedOutput, result);
        }
    }
}