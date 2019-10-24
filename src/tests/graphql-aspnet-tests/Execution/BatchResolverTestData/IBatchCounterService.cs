// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.BatchResolverTestData
{
    using System.Collections.Generic;

    public interface IBatchCounterService
    {
        IDictionary<string, int> CallCount { get; set; }
    }
}