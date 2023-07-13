// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeTemplates.InterfaceTestData
{
    public interface InterfaceWithUndeclaredInterfaceField
    {
        string PropFieldOnBaseInterface { get; set; }

        int MethodFieldOnBaseInterface(int param1);
    }
}