// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveInvocationTestData
{
    using GraphQL.AspNet.Attributes;

    public class SarcasticObject
    {
        [ApplyDirective(typeof(ToSarcasticCaseDirective))]
        public string Prop1 { get; set; }
    }
}