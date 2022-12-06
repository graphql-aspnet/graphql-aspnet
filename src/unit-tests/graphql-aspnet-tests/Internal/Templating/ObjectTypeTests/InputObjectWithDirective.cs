// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;

    [ApplyDirective(typeof(DirectiveWithArgs), 33, "input object arg")]
    public class InputObjectWithDirective
    {
        public int Prop1 { get; set; }
    }
}