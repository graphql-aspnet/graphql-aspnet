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

    [ApplyDirective(typeof(AddProperty3Directive))]
    public class TestObjectWithFieldExtensionDirectiveByType
    {
        public string Property1 { get; set; }

        public string Property2 { get; set; }
    }
}