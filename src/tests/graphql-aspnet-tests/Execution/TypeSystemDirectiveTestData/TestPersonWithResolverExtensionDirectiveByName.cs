// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TypeSystemDirectiveTestData
{
    using GraphQL.AspNet.Attributes;

    public class TestPersonWithResolverExtensionDirectiveByName
    {
        [ApplyDirective("ToUpper")]
        public string Name { get; set; }

        public string LastName { get; set; }
    }
}