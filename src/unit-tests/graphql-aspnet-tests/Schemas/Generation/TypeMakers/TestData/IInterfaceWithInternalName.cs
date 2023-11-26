// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Schemas.Generation.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;

    [GraphType(InternalName = "Interface_Internal_Name")]
    public interface IInterfaceWithInternalName
    {
        int Prop1 { get; set; }
    }
}