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

    public class ChildTestObject
    {
        [GraphField]
        public string ParentId { get; set; }

        [GraphField]
        public string Name { get; set; }
    }
}