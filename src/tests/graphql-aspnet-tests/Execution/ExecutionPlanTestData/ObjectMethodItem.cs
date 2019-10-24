// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.ExecutionPlanTestData
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;

    public class ObjectMethodItem
    {
        [GraphField]
        public int SyncMethod()
        {
            return 5;
        }

        [GraphField]
        public Task<int> AsyncMethod()
        {
            return 8.AsCompletedTask();
        }
    }
}