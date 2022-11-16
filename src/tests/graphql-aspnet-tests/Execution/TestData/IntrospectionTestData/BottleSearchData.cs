// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.IntrospectionTestData
{
    using GraphQL.AspNet.Attributes;

    [GraphType(InputName = "BottleSearch")]
    public class BottleSearchData
    {
        [GraphField]
        public string Name { get; set; }

        [GraphField]
        public int? Size { get; set; }
    }
}