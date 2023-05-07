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

    public interface InterfaceThatInheritsUndeclaredMethodField : InterfaceWithUndeclaredInterfaceField
    {
        string PropFieldOnInterface { get; set; }

        int MethodFieldOnInterface(int param1);
    }
}