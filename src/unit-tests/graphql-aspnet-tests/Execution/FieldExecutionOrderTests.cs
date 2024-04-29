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
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Tests.Execution.FieldExecutionorderTestData;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class FieldExecutionOrderTests
    {
        private static readonly Regex _whitespace = new Regex(@"\s+");

        [Test]
        public async Task OrderTest()
        {
            var server = new TestServerBuilder()
               .AddGraphQL(o =>
               {
                   o.AddType<HamburgerController>();
                   o.AddType<ChickenController>();
                   o.ResponseOptions.ExposeExceptions = true;
               })
               .Build();

            var builder = server.CreateQueryContextBuilder()
                .AddQueryText("query { " +
                "      retrieveDefaultChickenMeal {" +
                "            description " +
                "            name " +
                "            id" +
                "      }" +
                "      hamburger { " +
                "           retrieveOtherHamburgerMeal {" +
                "                 weight " +
                "                 name " +
                "                 id" +
                "                 description " +
                "           }" +
                "      }" +
                "      chicken { " +
                "           retrieveOtherChickenMeal {" +
                "                 id" +
                "                 description " +
                "                 name " +
                "           }" +
                "      }" +
                "      retrieveDefaultHamburgerMeal {" +
                "            id" +
                "            description " +
                "            name " +
                "            weight " +
                "      }" +
                "}");

            var expectedOutput =
                @"{
                  ""data"": {
                    ""retrieveDefaultChickenMeal"": {
                      ""description"": ""a top level chicken sandwich"",
                      ""name"": ""Chicken Bacon Ranch"",
                      ""id"": 5
                    },
                    ""hamburger"": {
                      ""retrieveOtherHamburgerMeal"": {
                        ""weight"": 1.3,
                        ""name"": ""Tiny Hamburger"",
                        ""id"": 5,
                        ""description"": ""a nested Hamburger""
                      }
                    },
                    ""chicken"": {
                      ""retrieveOtherChickenMeal"": {
                        ""id"": 5,
                        ""description"": ""a nested chicken sandwich"",
                        ""name"": ""Chicken Lettuce Tomato""
                      }
                    },
                    ""retrieveDefaultHamburgerMeal"": {
                      ""id"": 5,
                      ""description"": ""a top level hamburger"",
                      ""name"": ""Hamburger Supreme"",
                      ""weight"": 2.5
                    }
                  }
                }";

            var result = await server.RenderResult(builder);

            // must be an exact string with the exact character order for the data
            // dont use json matching just to be sure
            result = _whitespace.Replace(result, string.Empty);
            expectedOutput = _whitespace.Replace(expectedOutput, string.Empty);

            Assert.AreEqual(expectedOutput, result);
        }
    }
}