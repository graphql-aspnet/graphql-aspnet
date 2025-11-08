// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.IntrospectionTestData
{
    using GraphQL.AspNet.Attributes;

    [OneOf]
    [GraphType(InputName = "OneOfForIntrospection")]
    public class InputUnionWithOneOfForIntrospection
    {
        public string Prop1 { get; set; }

        public string Prop2 { get; set; }
    }
}