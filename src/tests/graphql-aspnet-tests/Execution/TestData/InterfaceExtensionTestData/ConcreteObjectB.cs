// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.InterfaceExtensionTestData
{
    using GraphQL.AspNet.Attributes;

    public class ConcreteObjectB : IInterfaceA
    {
        [GraphField]
        public string FirstName { get; set; }

        [GraphField]
        public string LastName { get; set; }

        [GraphField]
        public string Title { get; set; }
    }
}