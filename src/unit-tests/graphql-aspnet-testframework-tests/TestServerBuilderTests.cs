// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.TestFramework.Tests
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Tests.Framework;
    using NUnit.Framework;

    [TestFixture]
    public class TestServerBuilderTests
    {
        [Test]
        public void ParallelBuildSameControllerTest()
        {
            Task.WaitAll(
                Task.Run(BuildServer<AppController>),
                Task.Run(BuildServer<AppController>),
                Task.Run(BuildServer<AppController>),
                Task.Run(BuildServer<AppController>),
                Task.Run(BuildServer<AppController>),
                Task.Run(BuildServer<AppController>),
                Task.Run(BuildServer<AppController>),
                Task.Run(BuildServer<AppController>));
        }

        [GraphRoute("withParam")]
        public class AppController : GraphController
        {
            [Query("get")]
            public string Get(string id)
            {
                return id;
            }
        }

        private void BuildServer<T>()
            where T : GraphController
        {
            var builder = new TestServerBuilder<GraphSchema>();

            builder.AddGraphQL(options =>
            {
                options.AddController<T>();
            });

            builder.Build();
        }
    }
}