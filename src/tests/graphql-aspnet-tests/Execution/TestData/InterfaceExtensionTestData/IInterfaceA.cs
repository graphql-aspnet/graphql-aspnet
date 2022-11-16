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

    public interface IInterfaceA
    {
        [GraphField]
        string FirstName { get; set; }

        [GraphField]
        string LastName { get; set; }
    }
}