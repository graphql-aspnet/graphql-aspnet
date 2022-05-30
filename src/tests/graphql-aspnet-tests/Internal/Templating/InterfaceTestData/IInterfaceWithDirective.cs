// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Internal.Templating.InterfaceTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Tests.Internal.Templating.DirectiveTestData;

    [ApplyDirective(typeof(DirectiveWithArgs), 8, "big face")]
    public interface IInterfaceWithDirective
    {
        int Item1 { get; set; }
    }
}