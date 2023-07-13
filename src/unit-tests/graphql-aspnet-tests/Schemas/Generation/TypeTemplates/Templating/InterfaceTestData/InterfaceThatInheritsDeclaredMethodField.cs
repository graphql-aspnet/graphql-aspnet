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
    public interface InterfaceThatInheritsDeclaredMethodField : InterfaceWithDeclaredInterfaceField
    {
        string PropFieldOnInterface { get; set; }

        int MethodFieldOnInterface(int param1);
    }
}