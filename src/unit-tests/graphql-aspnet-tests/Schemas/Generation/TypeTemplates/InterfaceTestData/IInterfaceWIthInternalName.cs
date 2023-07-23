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

    [GraphType(InternalName = "MyInterface_32")]
    public interface IInterfaceWIthInternalName
    {
        [GraphField]
        string Property1 { get; }

        [GraphField]
        string Property2 { get; }
    }
}