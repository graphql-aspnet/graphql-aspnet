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
    using System.Collections.Generic;

    public class Blog
    {
        public int BlogId { get; set; }

        public string Url { get; set; }

        public virtual IList<BlogPost> Posts { get; set; }
    }
}