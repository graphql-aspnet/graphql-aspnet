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

    public interface InterfaceWithDeclaredInterfaceField
    {
        [GraphField]
        string PropFieldOnBaseInterface { get; set; }

        [GraphField]
        int FieldOnBaseInterface(int param1);
    }
}