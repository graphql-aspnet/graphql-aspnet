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
    using System;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;

    public class ObjectWithThrowMethod
    {
        [GraphField]
        public Task<string> ExecuteThrow()
        {
            throw new Exception("Failure from ObjectWithThrowMethod");
        }
    }
}