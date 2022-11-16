// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Defaults.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;

    [ApplyDirective(typeof(DirectiveWithArgs), 58, "interface arg")]
    public interface InterfaceWithDirective
    {
        public string Prop1 { get; set; }
    }
}