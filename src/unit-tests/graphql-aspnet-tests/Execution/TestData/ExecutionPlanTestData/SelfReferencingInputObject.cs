// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.ExecutionPlanTestData
{
    using System.Collections.Generic;

    public class SelfReferencingInputObject
    {
        public string Name { get; set; }

        public IEnumerable<SelfReferencingInputObject> Children { get; set; }
    }
}
