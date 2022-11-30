// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Configuration.SchemaOptionsTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    public class MutationAndQueryController : GraphController
    {
        [Query]
        public int Method1()
        {
            return 0;
        }

        [Mutation]
        public int Method2()
        {
            return 0;
        }
    }
}