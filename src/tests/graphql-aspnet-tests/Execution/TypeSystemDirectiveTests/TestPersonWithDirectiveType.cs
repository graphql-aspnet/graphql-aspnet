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

    public class TestPersonWithDirectiveType
    {
        [ApplyDirective(typeof(ToUpperDirective))]
        public string Name { get; set; }

        public string LastName { get; set; }
    }
}