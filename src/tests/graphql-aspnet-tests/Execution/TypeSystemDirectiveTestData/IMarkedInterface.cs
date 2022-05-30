// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTests
{
    using GraphQL.AspNet.Attributes;

    [ApplyDirective(typeof(InterfaceMarkerDirective))]
    public interface IMarkedInterface
    {
        string Prop1 { get; set; }
    }
}