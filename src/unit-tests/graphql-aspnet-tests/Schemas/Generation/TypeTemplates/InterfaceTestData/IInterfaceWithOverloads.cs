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
    using GraphQL.AspNet.Attributes;

    public interface IInterfaceWithOverloads
    {
        int Method1(int i);

        [GraphField]
        int Method1();
    }
}