// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ActionTestData
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    [GraphRoute("path0")]
    public class ContainerController : GraphController
    {
        [QueryRoot("path22")]
        [Description("MethodDescription")]
        public TwoPropertyObject RootMethod()
        {
            return null;
        }

        [Mutation("path9")]
        public Task<TwoPropertyObject> MethodWithParamsForGraphFieldCheck(string arg1, int arg2)
        {
            return Task.FromResult(new TwoPropertyObject());
        }
    }
}