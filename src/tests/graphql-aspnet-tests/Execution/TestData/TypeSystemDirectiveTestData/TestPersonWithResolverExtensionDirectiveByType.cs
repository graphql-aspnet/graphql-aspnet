// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.TypeSystemDirectiveTestData
{
    using GraphQL.AspNet.Attributes;

    public class TestPersonWithResolverExtensionDirectiveByType
    {
        [ApplyDirective(typeof(ToUpperFieldDefinitionDirective))]
        public string Name { get; set; }

        public string LastName { get; set; }
    }
}