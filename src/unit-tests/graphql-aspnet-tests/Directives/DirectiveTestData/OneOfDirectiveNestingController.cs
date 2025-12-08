// *************************************************************
//  project:  graphql-aspnet
//  --
//  repo: https://github.com/graphql-aspnet
//  docs: https://graphql-aspnet.github.io
//  --
//  License:  MIT
//  *************************************************************

namespace GraphQL.AspNet.Tests.Directives.DirectiveTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;

    public class OneOfDirectiveNestingController : GraphController
    {
        [QueryRoot(typeof(string))]
        public IGraphActionResult UnionInUnion(InputUnionWithChildAsInputUnion input)
        {
            return this.Ok("success");
        }

        [QueryRoot(typeof(string))]
        public IGraphActionResult TripleNestedUnion(InputUnionWithChildAsInputUnionThatHasChildAsInputUnion input)
        {
            return this.Ok("success");
        }

        [OneOf]
        [GraphType(InputName = "InputUnionItem")]
        public class InputUnionItem
        {
            public string Prop0 { get; set; }

            public int? Prop1 { get; set; }
        }

        [OneOf]
        [GraphType(InputName = "InputUnionWithChildAsInputUnion")]
        public class InputUnionWithChildAsInputUnion
        {
            public string Prop { get; set; }

            public InputUnionItem Child { get; set; }
        }

        [OneOf]
        [GraphType(InputName = "TripleNestedUnion")]
        public class InputUnionWithChildAsInputUnionThatHasChildAsInputUnion
        {
            public string Prop { get; set; }

            public InputUnionWithChildAsInputUnion Child { get; set; }
        }
    }
}