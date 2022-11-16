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
    using GraphQL.AspNet.Attributes;

    public class SyblingTestObject
    {
        [GraphField]
        public string SyblingId { get; set; }

        [GraphField]
        public string Name { get; set; }
    }
}